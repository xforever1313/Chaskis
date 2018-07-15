import hashlib
import io
import sys

def Checksum(target, source, env):
    with io.open(str(source[0]), 'rb') as inFile:
        readFile = inFile.read()
        h = hashlib.sha256(readFile)
        hash = h.hexdigest()

    with io.open(str(target[0]), 'w', encoding='utf-8') as outFile:
        outFile.write(ToUnicodeString(hash))

def ToUnicodeString(obj):
    # Need to use unicode for, well, unicode in python2,
    # but str in python3.
    if (sys.version_info.major == 2):
        return unicode(obj)
    else:
        return str(obj)