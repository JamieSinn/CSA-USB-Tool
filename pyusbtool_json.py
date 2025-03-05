#!/usr/bin/env python3
#
# Cross platform python script to download all the CSA tools to a specified
# directory
#

import argparse
import contextlib
import json
import hashlib
import pathlib
import urllib.request
import sys

USER_AGENT = "python-frc-csa-tool/1.0"
CHUNK_SIZE = 2**20


def download(url: str, dst_fname: pathlib.Path):
    """
    Downloads a file to a specified directory
    """

    def _reporthook(count, blocksize, totalsize):
        percent = int(count * blocksize * 100 / totalsize)
        if percent < 0 or percent > 100:
            sys.stdout.write("\r--%")
        else:
            sys.stdout.write("\r%02d%%" % percent)
        sys.stdout.flush()

    print("Downloading", url)

    request = urllib.request.Request(url, headers={"User-Agent": USER_AGENT})

    with contextlib.closing(urllib.request.urlopen(request)) as fp:
        headers = fp.info()

        with open(dst_fname, "wb") as tfp:
            # copied from urlretrieve source code, Python license
            bs = 1024 * 8
            size = -1
            blocknum = 0
            read = 0
            if "content-length" in headers:
                size = int(headers["Content-Length"])

            while True:
                block = fp.read(bs)
                if not block:
                    break
                read += len(block)
                tfp.write(block)
                blocknum += 1
                _reporthook(blocknum, bs, size)

    sys.stdout.write("\n")
    sys.stdout.flush()


def md5_file(fname: pathlib.Path) -> str:
    with open(fname, "rb") as fp:
        h = hashlib.md5()
        chunk = fp.read(CHUNK_SIZE)
        while chunk:
            h.update(chunk)
            chunk = fp.read(CHUNK_SIZE)

    return h.hexdigest()


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("json", type=pathlib.Path, help="Specifies the JSON file to read from")
    parser.add_argument("dst", type=pathlib.Path, help="Specifies the destination directory")
    parser.add_argument(
        "-d",
        "--download",
        action="store_true",
        default=False,
        help="Download files to disk (default is to check only)",
    )
    parser.add_argument(
        "--no-verify",
        action="store_true",
        default=False,
        help="Don't verify md5sum of existing files",
    )
    args = parser.parse_args()

    present = 0
    missing = 0
    invalid = 0

    with open(args.json) as fp:
        data = json.load(fp)
        print("Loaded ", args.json, "Year=", data['Year'], "Program=", data['Program'], "Software=", len(data['Software']))
        for record in data["Software"]:
            (name, fname, url, md5) = [ record.get(key) for key in ["Name", "FileName", "Uri", "Hash"] ]
            if fname is None or url is None:
                print("Skipping", name, "because it lacks filename or URL")
                continue

            valid_checksum = md5 is not None
            if valid_checksum:
                md5 = md5.lower()

            fname = args.dst / fname
            is_invalid = False
            if fname.exists():
                if not valid_checksum:
                    print(name, "exists and has no checksum")
                    present += 1
                    continue
                elif args.no_verify:
                    print(name, "exists")
                    present += 1
                    continue
                else:
                    md5_actual = md5_file(fname)
                    if md5_actual == md5:
                        print(name, "exists and has valid checksum")
                        present += 1
                        continue
                    else:
                        print(name, "exists but has invalid checksum", "actual:", md5_actual, "expected:", md5)
                is_invalid = True

            if args.download:
                download(url, fname)
                if valid_checksum:
                    md5_actual = md5_file(fname)
                    if md5_actual != md5:
                        print(name, "does not match checksum",  "actual:", md5_actual, "expected:", md5)
                        invalid += 1
                    else:
                        present += 1
                else:
                    present += 1
            else:
                if is_invalid:
                    print(name, "does not match checksum")
                    invalid += 1
                else:
                    print(name, "is missing")
                    missing += 1

    print()
    print("Finished!")
    print("-", present, "OK")
    print("-", missing, "missing")
    print("-", invalid, "invalid")
