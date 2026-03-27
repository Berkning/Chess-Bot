<h1 align="center">
Manatee - A C# chess engine
</h1>

<h2 align="center">
<img width="500" height="500" alt="ManateeLarge" src="https://github.com/user-attachments/assets/7623a63a-c8c2-4f5d-8b2c-bb2a4c363424" />
</h2>

A C# chess engine created by me in my spare time. Using [OpenBench](https://github.com/AndyGrant/OpenBench) for SPRT testing.


## Features
* **Search**
  * Alpha-beta search
  * Quiescence Search
  * Iterative Deepening
  * Transposition Table
  * Aspiration Windows
  * Move ordering
    * Hash Move
    * Killer moves
    * History
    * Basic MVV-LVA
  * Promotion Extensions
  * Late Move Reductions (LMR)
* **Evaluation**
  * SGD Tuned Hand-Crafted Eval
  * Tapered Eval
* **Move-Generation**
  * Fully Legal Move Generator
  * Magic Bitboards
* **Multi-Threading**
  * Lazy-SMP
  * Helper Threads
* **Misc.**
  * Opening Book
  * ASCII Board Drawer
  * Perft
  * Bench
 
## Rating
**CCRL**: Currently not on CCRL, but i am working towards it.

**Lichess**: Around **2000-2050** rated in Bullet and Blitz. Normally slightly higher in Rapid

## Contributing
If you have any ideas, feel free to open an issue or even a pull request.
