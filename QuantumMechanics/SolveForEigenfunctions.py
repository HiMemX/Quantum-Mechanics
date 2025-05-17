
from scipy.sparse.linalg import eigsh
from scipy.sparse import csr_matrix
import struct
import numpy as np

import datetime

t1 = datetime.datetime.now()

with open("siminfo.dat", "rb") as file:
    desiredCount = int.from_bytes(file.read(4), "little")

    hbar = struct.unpack("<d", file.read(8))[0]
    mass = struct.unpack("<d", file.read(8))[0]
    domainLength = struct.unpack("<d", file.read(8))[0]
    spacialResolution = int.from_bytes(file.read(4), "little")

    potential = [[struct.unpack("<d", file.read(8))[0] for x in range(spacialResolution)] for y in range(spacialResolution)]

ds = domainLength / spacialResolution

H = np.zeros((spacialResolution**2, spacialResolution**2))

scalar =1# hbar**2 / (2*mass) / ds**2

for y in range(spacialResolution):
    for x in range(spacialResolution):
        row = y * spacialResolution + x

        H[row, row] = (2 * hbar**2 / (mass * ds**2) + potential[y][x]) / scalar

        if (x+1) < spacialResolution: H[row, row+1] = -hbar**2 / (2 * mass * ds**2) /scalar
        if (y+1) < spacialResolution: H[row, row+spacialResolution] = -hbar**2 / (2 * mass * ds**2)/scalar
        
        if x != 0: H[row, row-1] = -hbar**2 / (2 * mass * ds**2)/scalar
        if y != 0: H[row, row-spacialResolution] = -hbar**2 / (2 * mass * ds**2)/scalar

print(H)

H = csr_matrix(H)


eigenvalues, eigenvectors = eigsh(H, desiredCount, which="SM")

with open("computed.dat", "wb") as file:
    for i in range(desiredCount):
        file.write(struct.pack("<d", eigenvalues[i])) # Actual eigenvalues are scalar * value
        
    for i in range(desiredCount):
        [file.write(struct.pack("<d", eigenvectors[k, i])) for k in range(spacialResolution*spacialResolution)]

        
t2 = datetime.datetime.now()

print(f"Computed {desiredCount} eigenfunctions in {(t2-t1).microseconds / 1_000_000 + (t2-t1).seconds}s")