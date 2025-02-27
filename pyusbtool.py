#!/usr/bin/env python
import os
import sys
import hashlib
import requests
import json
import argparse

# Try importing tqdm for progress bars
try:
    from tqdm import tqdm
    TQDM_AVAILABLE = True
except ImportError:
    TQDM_AVAILABLE = False

VERSION = "1.0.1"

def compute_md5(file_path, chunk_size=4096):
    """Compute the MD5 hash of a file with optional progress bar."""
    md5 = hashlib.md5()
    try:
        file_size = os.path.getsize(file_path)
        progress_bar = None

        if TQDM_AVAILABLE:
            progress_bar = tqdm(total=file_size, unit='B', unit_scale=True, desc=f"Verifying {os.path.basename(file_path)}")

        with open(file_path, 'rb') as f:
            while True:
                chunk = f.read(chunk_size)
                if not chunk:
                    break
                md5.update(chunk)
                if progress_bar:
                    progress_bar.update(len(chunk))

        if progress_bar:
            progress_bar.close()

        return md5.hexdigest()
    except IOError as e:
        print(f"Error reading file {file_path}: {e}")
        return None

def download_file(url, file_path):
    """Download a file from a URL with an optional progress bar."""
    try:
        response = requests.get(url, stream=True)
        response.raise_for_status()
        total_size = int(response.headers.get('content-length', 0))

        progress_bar = None
        if TQDM_AVAILABLE:
            progress_bar = tqdm(total=total_size, unit='B', unit_scale=True, desc=f"Downloading {os.path.basename(file_path)}")

        with open(file_path, 'wb') as f:
            for chunk in response.iter_content(chunk_size=8192):
                if chunk:
                    f.write(chunk)
                    if progress_bar:
                        progress_bar.update(len(chunk))

        if progress_bar:
            progress_bar.close()

        print(f"Download complete: {file_path}")
    except requests.RequestException as e:
        print(f"Error downloading {file_path} from {url}: {e}")

def main():
    parser = argparse.ArgumentParser(
        description="Download and verify software files from a JSON configuration."
    )
    parser.add_argument("config", help="Path to the JSON configuration file.")
    parser.add_argument(
        "destination", nargs="?", default=os.getcwd(),
        help="Destination directory for downloaded files (default: current directory)."
    )
    parser.add_argument(
        "-s", "--skip", action="store_true",
        help="Skip MD5 checksum validation for existing files."
    )
    parser.add_argument("-v", "--version", action="store_true", help="Show script version and exit.")

    args = parser.parse_args()

    if args.version:
        print(f"Script Version: {VERSION}")
        sys.exit(0)

    config_filename = args.config
    destination_directory = args.destination
    skip_md5 = args.skip

    # Ensure the destination directory exists
    os.makedirs(destination_directory, exist_ok=True)

    # Load configuration from the provided JSON file
    try:
        with open(config_filename, 'r') as f:
            config = json.load(f)
    except Exception as e:
        print(f"Failed to load configuration file '{config_filename}': {e}")
        sys.exit(1)

    for software in config.get("Software", []):
        file_name = software.get("FileName")
        expected_hash = software.get("Hash")
        uri = software.get("Uri")

<<<<<<< Updated upstream
        # Skip entries without a filename (or where no file is expected)
        if file_name is None or expected_hash is None:
=======
        # Skip entries without a filename; allow missing hash values.
        if file_name is None:
>>>>>>> Stashed changes
            print(f"Skipping '{software.get('Name', 'Unknown')}' as no file is specified.")
            continue

        file_path = os.path.join(destination_directory, file_name)
<<<<<<< Updated upstream

        file_exists = os.path.exists(file_path)

        if file_exists and skip_md5:
            print(f"Skipping MD5 validation for '{file_path}' as per user request.")
            continue

        if file_exists:
            current_hash = compute_md5(file_path)
            if current_hash and current_hash.lower() == expected_hash.lower():
                print(f"'{file_path}' exists and the hash matches.")
                continue
            else:
                print(f"'{file_path}' exists but the hash does not match (expected {expected_hash}, got {current_hash}).")
=======
        file_exists = os.path.exists(file_path)

        if file_exists:
            # If no hash is provided, skip validation.
            if expected_hash is None:
                print(f"'{file_path}' exists; no hash provided, skipping hash validation.")
                continue
            elif skip_md5:
                print(f"Skipping MD5 validation for '{file_path}' as per user request.")
                continue
            else:
                current_hash = compute_md5(file_path)
                if current_hash and current_hash.lower() == expected_hash.lower():
                    print(f"'{file_path}' exists and the hash matches.")
                    continue
                else:
                    print(f"'{file_path}' exists but the hash does not match (expected {expected_hash}, got {current_hash}).")
>>>>>>> Stashed changes
        else:
            print(f"'{file_path}' does not exist.")

        # Download or re-download the file
        try:
            download_file(uri, file_path)
<<<<<<< Updated upstream
            if not skip_md5:
=======
            if expected_hash is not None and not skip_md5:
>>>>>>> Stashed changes
                downloaded_hash = compute_md5(file_path)
                if downloaded_hash and downloaded_hash.lower() == expected_hash.lower():
                    print(f"Successfully verified the downloaded file '{file_path}'.\n")
                else:
                    print(f"Hash mismatch after downloading '{file_path}'. Expected {expected_hash}, got {downloaded_hash}.\n")
<<<<<<< Updated upstream
=======
            else:
                print(f"Downloaded '{file_path}' with no hash verification.\n")
>>>>>>> Stashed changes
        except Exception as e:
            print(f"Error processing '{file_path}': {e}")

if __name__ == '__main__':
<<<<<<< Updated upstream
    main()
=======
    main()
>>>>>>> Stashed changes
