
import torch
from transformers import AutoModelForCausalLM, AutoTokenizer
from huggingface_hub import snapshot_download


DEVICE = "cuda" if torch.cuda.is_available() else "cpu"


class ModelLoader:
    def __init__(self, name, offline=False):
        self.model_name_ = name
        self.offline_ = offline

    def download(self, local_dir):
        """Download model snapshot into local directory"""
        snapshot_download(self.model_name_, local_dir=local_dir, local_dir_use_symlinks=False)

    def load_tokenizer(self):
        """Load and instantiate pretrained tokenizer"""
        return AutoTokenizer.from_pretrained(self.model_name_, local_files_only=self.offline_)
    
    def load_model(self):
        """Load and instantiate pretrained language model"""
        return AutoModelForCausalLM.from_pretrained(self.model_name_, local_files_only=self.offline_).to(DEVICE)

