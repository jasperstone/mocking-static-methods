# Model Tuning for C# Code Generation


## Downloading Models and Datasets

**Huggingface Authentication**

Log into Huggingface to access models and datasets.

```sh
hf auth login --token <token>
```

**Set Cache**

HF_HOME sets location of huggingface cache for downloaded models and
datasets. By default, `.cache` in the `$HOME` directory is used, which
will quickly blow the user quota on ptolemy. So it is set to /scratch.
Replace <userid> as necessary.

```sh
export HF_HOME=/scratch/ptolemy/users/<userid>/.cache
```

**Datasets**

- Azamorn/tiny-codes-csharp
- kloodia/c-sharp_200k


**Models**
- google/gemma-3-270m
- google/gemma-3-1b-it
- google/gemma-3-1b-pt


**Download Command**

Datasets and base models are downloaded using the `download` subcommand.

For example:

```sh
cst download -m google/gemma-3-1b-pt -d Azamorn/tiny-codes-csharp
```

## Tuning Models

Tuning models retrains base models using a low learning rate. Options such as
number of epochs, sample size, learning rate, model, and dataset are configurable
through command line options.

For example, this command tunes the gemma-3-270m model for 1 epoch with 1000 samples of
the Azamorn dataset, uses offline training, and saves the tuned model into `g270m_tuned`:

```sh
cst tune -m google/gemma-3-270m -e 1 -s 1000 -o -d Azamorn/tiny-codes-csharp g270m_tuned
```

## Interactive Chat

Models can be used for interactive chat as such:

```sh
cst chat google/gemma-3-1b-it/ 
```


## Batch Processing

Multiple files can be processed at once using the `batch` subcommand. This example will
provide each of the files in the `prompts` directory to the named model, saving the
result in the `Output` directory. Each input file contains a single prompt and a single
output file will be created for each prompt.

```sh
cst batch -m google/gemma-3-1b-it Output prompts/*
```
