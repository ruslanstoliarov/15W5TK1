
import os
import re
import argparse

"""
The Context Deserializer

Purpose:
This script allows the AI to write files to your hard drive.
When the AI generates a code block containing multiple files (in the 
Standard Serializable Format), you paste that block into a text file,
and this script splits it up and saves the individual files.

The Format it expects:
# Project: Name; File: /path/to/file.txt
[File Content]
---[END OF FILE: /path/to/file.txt]---

Usage:
python Tools/AI_Serialized-To-VSC_Files.py SERIALIZED_CONTEXT.txt
"""

def strip_markdown_artifacts(content):
    """
    Removes Markdown code block wrappers (``` or ```text) if the user
    accidentally copied them along with the content.
    """
    content = content.strip()
    
    # Remove opening ```
    if content.startswith("```"):
        content = re.sub(r"^```[^\n]*\n", "", content)
        
        # Remove closing ```
        if content.endswith("```"):
            content = content[:-3].strip()
            
    return content

def parse_serialized_context(input_file_path):
    """
    Reads the big text file and uses Regex to find the start and end
    of each file block.
    """
    files_data = []
    try:
        with open(input_file_path, 'r', encoding='utf-8') as f:
            raw_content = f.read()

        # Clean up any markdown wrappers
        clean_content = strip_markdown_artifacts(raw_content)

        # Regex to match the Header: # Project: ...; File: ...
        # We use multiline mode to find headers anywhere in the file
        header_pattern = re.compile(r"^(?:\*\*)?#\s*Project:.*;\s*File:\s*(.*?)(?:\*\*)?\s*$", re.MULTILINE)
        
        matches = list(header_pattern.finditer(clean_content))
        
        for i, match in enumerate(matches):
            file_path = match.group(1).strip()
            start_index = match.end()
            
            # Construct the expected footer string for this specific file
            footer_string = f"---[END OF FILE: {file_path}]---"
            
            # Search for the footer starting from where the header ended
            footer_pos = clean_content.find(footer_string, start_index)
            
            if footer_pos != -1:
                # Extract the content between header and footer
                file_content = clean_content[start_index:footer_pos].strip()
                
                if file_path:
                    files_data.append((file_path, file_content))
            else:
                print(f"WARNING: Found header for {file_path} but no matching footer. Skipping.")

        return files_data

    except Exception as e:
        print(f"ERROR: Failed to parse input file: {e}")
        return []

def write_files_to_workspace(files_data, repo_root):
    """
    Takes the parsed data and actually writes the files to disk.
    It will create directories if they don't exist.
    """
    if not files_data:
        print("No valid file blocks found to write.")
        return

    print(f"Writing files to: {repo_root}")

    for file_path, content in files_data:
        # Clean up the path (remove leading slashes to make it relative)
        relative_path = file_path.lstrip('/').lstrip('\\')
        absolute_path = os.path.join(repo_root, relative_path)

        try:
            # Create parent directories
            os.makedirs(os.path.dirname(absolute_path), exist_ok=True)

            # Write the content (Overwrites existing files!)
            with open(absolute_path, 'w', encoding='utf-8') as f:
                f.write(content)

            print(f"Updated: {relative_path}")

        except Exception as e:
            print(f"ERROR writing {relative_path}: {e}")

if __name__ == "__main__":
    # Set up command line arguments
    parser = argparse.ArgumentParser(description="Splits a serialized text file into actual files.")
    parser.add_argument("input_file", help="Path to the text file containing the code blocks (e.g., SERIALIZED_CONTEXT.txt)")
    args = parser.parse_args()

    # Assume script is run from project root
    REPO_ROOT = os.getcwd()
    
    input_file_abs_path = os.path.abspath(args.input_file)
    
    # Run the parser
    parsed_data = parse_serialized_context(input_file_abs_path)
    
    # Run the writer
    write_files_to_workspace(parsed_data, REPO_ROOT)