#!/bin/bash
## This line requests that the job be allocated one node
#SBATCH -N 1

## This line requests one task for the job
#SBATCH -n 1

## This specifies the amount of memory required for the job. 
## Here, it's set to 10 Gigabytes. This memory is allocated per node.
#SBATCH --mem=10G

## This sets the partition (or queue) to gpu-a100
## Partitions are defined by the system administrators and 
## determine the set of nodes the job can run on. 
#SBATCH -p gpu-a100

## This specifies the account for job charging or accounting. 
## Here, it's set to an account named 'test'. To find out
## which account you should use, run “sacctmgr show association where user=jhb11” and it
## will show the various accounts available to you. If you get an empty list, be sure
## to email help@hpc.msstate.edu to request the association of your account with an
## appropriate “billing account”
#SBATCH -A class-cse8990

## This sets the time limit for the job. 
## Here, the job can run for up to 1 hour. 
## If the job exceeds this limit, SLURM will terminate it.
#SBATCH -t 12:00:00

## This requests a specific generic resource (GRES), in this case, 
## a GPU. Specifically, it requests one Nvidia A100 GPU with 10 GB of memory.
#SBATCH --gres=gpu:a100:1

## This sets the name of the job to "jhb11-GPU-mnist". 
## This name will appear in the queue and in various SLURM reports, 
## helping you to identify your job.
#SBATCH --job-name "model-training"

## This specifies that all the output from the job (stdout) will 
## be written to the file output.out. This file will be created in the 
## directory from which the SLURM script is submitted.
#SBATCH --output=output.out
#SBATCH --error=error.out

## each of these lines uses the "module load" command
## to load the modules our python script will need to use
ml cuda
ml python

# Replace <userid> as necessary
export USERID=<userid>
export HF_HOME=/scratch/ptolemy/users/$USERID/Project/.cache
export HF_HUB_OFFLINE=1

## Set a path you can use inside your python script to find your data files
export DATA_FILE_PATH="/scratch/ptolemy/users/$USERID"

## change directories to your user scratch location where your 
## script is
cd $DATA_FILE_PATH

## activate environment
source .venv/bin/activate

## run the script
cst tune -m google/gemma-3-270m -e 1 -s 1000 -o -d Azamorn/tiny-codes-csharp g270m_tuned
