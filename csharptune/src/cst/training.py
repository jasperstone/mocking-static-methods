
def get_tokenizer_fn(dataset_name):
    return {
        "azamorn/tiny-codes-csharp": tokenize_azamorn,
        "kloodia/c-sharp_200k": tokenize_kloodia,
    }.get(dataset_name.lower())


def tokenize_kloodia(tokenizer, dataset):
    def tokenize(examples):
        return tokenizer(examples["text"], padding="max_length", max_length=2048, truncation=True)

    dataset = dataset.map(tokenize, batched=True)
    return dataset


def tokenize_azamorn(tokenizer, dataset):
    def tokenize(example):
        ex = [{"role": "user", "content": example["instruction"]}, {"role": "assistant", "content": example["output"]}]
        formatted =  tokenizer.apply_chat_template(ex, add_generation_prompt=False, tokenize=False)
        return tokenizer(formatted, padding="max_length", max_length=2048, truncation=True)

    dataset = dataset.map(tokenize, batched=False, remove_columns=["instruction", "output"])
    return dataset


def create_trainer(model, tokenizer, dataset, output_dir, epochs, learning_rate, format_fn):
    from transformers import TrainingArguments, Trainer, DataCollatorForLanguageModeling

    if not tokenizer.chat_template:
        tokenizer.chat_template = "{% if not add_generation_prompt is defined %}{% set add_generation_prompt = false %}{% endif %}{% for message in messages %}{{'<bos>' + message['role'] + '\n' + message['content'] + '<eos>' + '\n'}}{% endfor %}{% if add_generation_prompt %}{{ '<bos>assistant\n' }}{% endif %}"
    tokenizer.pad_token = tokenizer.eos_token

    dataset = format_fn(tokenizer, dataset)

    data_collator = DataCollatorForLanguageModeling(tokenizer=tokenizer, mlm=False)

    args = TrainingArguments(
        output_dir=output_dir,
        num_train_epochs=epochs,
        per_device_train_batch_size=4,
        gradient_accumulation_steps=1,
        gradient_checkpointing=True,        
        eval_strategy="no",
        save_strategy="epoch",
        learning_rate=learning_rate,
        push_to_hub=False)

    trainer = Trainer(
        model=model,
        args=args,
        train_dataset=dataset['train'],
        data_collator=data_collator,
    )

    return trainer
