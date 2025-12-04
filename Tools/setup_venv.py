
import os
import sys
import subprocess
import platform

"""
Virtual Environment Setup Tool

Purpose:
Creates a focused Python environment (.venv) for this project.
This ensures your project tools run correctly without interfering
with other programs on your computer.

Usage:
python Tools/setup_venv.py
"""

def create_venv():
    # 1. Define the venv directory
    venv_dir = os.path.join(os.getcwd(), ".venv")
    
    # 2. Check if it already exists
    if os.path.exists(venv_dir):
        print(f"✅ Virtual environment already exists at: {venv_dir}")
    else:
        print(f"⚙️  Creating virtual environment at: {venv_dir}...")
        try:
            # Run the standard python venv module
            subprocess.check_call([sys.executable, "-m", "venv", ".venv"])
            print("✅ Virtual environment created successfully.")
        except subprocess.CalledProcessError:
            print("❌ Error: Failed to create virtual environment.")
            return

    # 3. Install requirements (if they exist)
    req_file = "requirements.txt"
    if os.path.exists(req_file):
        print(f"📦 Installing dependencies from {req_file}...")
        
        # Determine the path to the pip executable inside the venv
        if platform.system() == "Windows":
            pip_exe = os.path.join(venv_dir, "Scripts", "pip")
        else:
            pip_exe = os.path.join(venv_dir, "bin", "pip")
            
        try:
            subprocess.check_call([pip_exe, "install", "-r", req_file])
            print("✅ Dependencies installed.")
        except Exception as e:
            print(f"⚠️  Warning: Could not install dependencies. Error: {e}")
            print("You may need to install them manually.")
    
    # 4. Instructions for the Student
    print("\n" + "="*50)
    print("🎉 SETUP COMPLETE!")
    print("="*50)
    print("To make VS Code use this environment:")
    print("1. Press 'Ctrl + Shift + P' (Cmd+Shift+P on Mac)")
    print("2. Type: 'Python: Select Interpreter'")
    print("3. Choose the one marked ('.venv': venv)")
    print("="*50)

if __name__ == "__main__":
    create_venv()