import hashlib
import io

def Checksum(target, source, env):
    with io.open(str(source[0]), 'rb') as inFile:
        readFile = inFile.read()
        h = hashlib.sha256(readFile)
        hash = h.hexdigest()

    with io.open(str(target[0]), 'w', encoding='utf-8') as outFile:
        outFile.write(str(hash))