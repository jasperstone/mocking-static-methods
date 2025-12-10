import sys
import os
import click

from pathlib import Path


DEFAULT_DATASET = "kloodia/c-sharp_200k"
DEFAULT_MODEL = "google/gemma-3-270m"


@click.group()
def main():
    """Finetuning models for C# code generation"""


@main.command("chat")
@click.argument('model-name')
@click.argument('input-file', type=click.File('r'), default=sys.stdin)
def main_chat(model_name, input_file):
    """Interactive conversation with pretrained model

    By default, this command starts an interactive chat session with the
    named model. If an input file is provided, it will be processed instead.
    """

    from .models import ModelLoader

    loader = ModelLoader(model_name, offline=True)
    tokenizer = loader.load_tokenizer()
    model = loader.load_model()

    for line in input_file:
        model_inputs = [line]
        if tokenizer.chat_template:
            prompt = [{"role": "user", "content": line}]
            model_inputs = tokenizer.apply_chat_template(prompt, tokenize=False, add_generation_prompt=True)
        
        model_inputs = tokenizer(model_inputs, return_tensors="pt").to(model.device)

        generated_ids = model.generate(**model_inputs, max_new_tokens=2048, num_beams=3, do_sample=True, no_repeat_ngram_size=2, early_stopping=True)

        click.echo(tokenizer.batch_decode(generated_ids, skip_special_tokens=True)[0])


@main.command("batch")
@click.option('--model', '-m', default=DEFAULT_MODEL, help='Pretrained Model')
@click.argument('output-path', type=click.Path(exists=False, file_okay=False))
@click.argument('input-files', type=click.Path(exists=True, dir_okay=False), nargs=-1)
def main_batch(model, output_path, input_files):
    """Batch process provided prompts

    Apply the named model to each of the specified input files, writing the result
    into separate files in the "output-path" directory.
    """

    from .models import ModelLoader

    loader = ModelLoader(model, offline=True)
    tokenizer = loader.load_tokenizer()
    model = loader.load_model()

    output_path = Path(output_path)
    output_path.mkdir(parents=True)

    for input_file in input_files:
        output = []

        with open(input_file) as file:
            for line in file:
                model_inputs = [line]
                if tokenizer.chat_template:
                    prompt = [{"role": "user", "content": line}]
                    model_inputs = tokenizer.apply_chat_template(prompt, tokenize=False, add_generation_prompt=True)

                model_inputs = tokenizer(model_inputs, return_tensors="pt").to(model.device)
                generated_ids = model.generate(**model_inputs, max_new_tokens=2048, num_beams=3, do_sample=True, no_repeat_ngram_size=2, early_stopping=True)
                output.append(tokenizer.batch_decode(generated_ids)[0])

        if len(output) == 0:
            continue

        with open(output_path / Path(input_file).name, "w") as out:
            out.write('\n'.join(output))


@main.command("download")
@click.option('--model', '-m', default=DEFAULT_MODEL, help='Pretrained model')
@click.option('--model-dir', help='Downloaded model directory')
@click.option('--dataset', '-d', default=DEFAULT_DATASET, help="Fine-tuning dataset")
def main_download(model, model_dir, dataset):
    """Download tuning dataset and pretrained models
    
    Pre-download models and datasets for offline training and inference.
    """

    from datasets import load_dataset
    from .models import ModelLoader

    if not model_dir:
        model_dir = model

    load_dataset(dataset)

    loader = ModelLoader(model)
    loader.download(model_dir)


@main.command("tune")
@click.option('--model', '-m', default=DEFAULT_MODEL, help='Pretrained model')
@click.option('--epochs', '-e', default=3, show_default=True, help='Number of tuning epochs')
@click.option('--sample-size', '-s', default=None, type=int, show_default=True, help='Sample size')
@click.option('--offline', '-o', is_flag=True, help="Offline training")
@click.option('--dataset', '-d', default=DEFAULT_DATASET, help="Fine-tuning dataset")
@click.option('--learning-rate', '-l', default=1e-5, help="Learning rate")
@click.argument('output-dir', type=click.Path(exists=False))
def main_tune(model, epochs, sample_size, offline, dataset, learning_rate, output_dir):
    """Tune model with given dataset
    
    Fine-tune model for text generation. Models are saved to the specified "output-dir".
    """

    if offline:
        os.environ['HF_DATASETS_OFFLINE'] = '1'

    from datasets import load_dataset
    from .models import ModelLoader
    from .training import create_trainer, get_tokenizer_fn

    ds = load_dataset(dataset)

    loader = ModelLoader(model, offline=offline)
    tokenizer = loader.load_tokenizer()
    model = loader.load_model()

    if sample_size is not None:
        ds["train"] = ds["train"].shuffle(seed=42).select(range(sample_size))

    format_fn = get_tokenizer_fn(dataset)

    trainer = create_trainer(model, tokenizer, ds, output_dir, epochs, learning_rate, format_fn)
    trainer.train()
