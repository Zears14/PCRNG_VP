## Bugs

### Critical Severity

* **Failure to encrypt/decrypt data with size more than ~800MB (CR01)** (@Zears14)
    * **Description:** When the condition of this bug is met, the program will crash with OutOfMemory exception.
    * **Impact:** Crash due to OutOfMemory exception.

### High Severity


### Medium Severity


### Low Severity


## To-Do List

### High Priority

1. [ ] **Fix CR01: Encryption/Decryption for Large Files** (CR01)
    * Implement a solution to handle data sizes exceeding 800MB during encryption/decryption.

2. [ ] **Implement Caching System for Password or the key**
    * Design and implement a caching system to reduce password prompts for subsequent cryptographic operations within a designated timeframe. 

