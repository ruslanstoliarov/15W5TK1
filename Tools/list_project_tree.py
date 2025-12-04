
import os
import sys

"""
Project File Finder (The "Map Maker")

Purpose:
This script scans your project directory and creates a clean list of all 
relevant files. This list is used by the 'ReConstructor' to build the 
AI's context.

How it works:
1. It walks through every folder in your project.
2. It ignores 'junk' folders (like .git, __pycache__, node_modules).
3. It ignores binary files (images, zips) that the AI can't read.
4. It prints the clean paths to the console (standard output).

Usage:
python Tools/list_project_tree.py > reconstructor-file-list.txt
"""

def list_project_files(startpath):
    """
    Scans the project directory and prints a clean list of file paths.
    Intended to be piped into 'reconstructor-file-list.txt'.
    """

    # --- Configuration: Directories to ignore ---
    # We exclude .git (version control internals), .venv (python libraries),
    # and X_Intels (external teacher repos which are too big to read fully).
    ignore_dirs = {
        '.git', '.venv', '__pycache__', '.vscode', 'node_modules', 
        'build', 'dist', 'Tools', 'X_Intels', 'X_Repos'
    }
    
    # --- Configuration: Files to ignore ---
    # We exclude system files (.DS_Store) and the context files themselves
    # (to prevent infinite loops of the AI reading its own memory dump).
    ignore_files = {
        '.DS_Store', 'Thumbs.db', 
        'SERIALIZED_CONTEXT.txt', 'RECONSTRUCTOR_CONTEXT.md',
        'reconstructor-file-list.txt', '_SESSION_ADDENDUM_BUFFER.md'
    }
    
    # --- Configuration: Extensions to ignore ---
    # The AI cannot read binary files, so we skip them to save space.
    ignore_extensions = {
        '.png', '.jpg', '.jpeg', '.gif', '.ico', 
        '.pdf', '.zip', '.7z', '.exe', '.dll', 
        '.pyc', '.blend', '.fbx', '.obj', '.json'
    }

    # Walk the directory tree
    for root, dirs, files in os.walk(startpath, topdown=True):
        # Modify dirs in-place to prevent os.walk from visiting ignored directories
        dirs[:] = [d for d in dirs if d not in ignore_dirs]

        for name in files:
            if name in ignore_files:
                continue
            
            _, ext = os.path.splitext(name)
            if ext.lower() in ignore_extensions:
                continue
                
            # Construct the full path
            full_path = os.path.join(root, name)
            
            # Create a relative path (e.g., "0_Meta/README.md" instead of "C:/Users/...")
            relative_path = os.path.relpath(full_path, startpath)
            
            # Normalize to forward slashes (/) for compatibility across Windows/Mac/Linux
            clean_path = relative_path.replace(os.sep, '/')
            
            print(clean_path)

if __name__ == "__main__":
    # Default to current working directory
    project_root = "."
    
    # If an argument is provided (e.g., python script.py /path/to/repo), use that
    if len(sys.argv) > 1:
        project_root = sys.argv[1]
        
    list_project_files(project_root)