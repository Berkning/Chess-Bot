<h1 align="center">
Manatee - A C# Chess Engine
</h1>

<h2 align="center">
<img width="500" height="500" alt="ManateeLarge" src="https://github.com/user-attachments/assets/7623a63a-c8c2-4f5d-8b2c-bb2a4c363424" />
</h2>

A C# chess engine created by me in my spare time. Using [OpenBench](https://github.com/AndyGrant/OpenBench) for SPRT testing. All code is written by hand and maintained by me.

## Features

- **Search**
  - Alpha-beta search
  - Quiescence Search
  - Iterative Deepening
  - Transposition Table
  - Aspiration Windows
  - Check Extensions
  - Promotion Extensions
  - Late Move Reductions (LMR)
  - Null Move Pruning (NMP)
  - Razoring
  - ... and additional stuff i am probably forgetting
- **Evaluation**
  - Hand-Crafted Evaluation (HCE)
    - Tuned With Custom SGD Trainer
  - Tapered Eval
- **Move-Generation**
  - Fully Legal Move Generator
  - Magic Bitboards
  - PEXT Bitboards
- **Move-Ordering**
  - Hash Move
  - Killer moves
  - History
  - Basic MVV-LVA
  - Lazy/deferred ordering
  - ... and some custom additions
- **Multi-Threading**
  - Lazy-SMP
  - Helper Threads
- **Misc.**
  - Tunable Parameters (SPSA)
  - Custom Internal Opening Book
  - ASCII Board Drawer
  - Perft
  - Bench
  - Diagnostics

## Rating

**CCRL**: Currently not on CCRL, but i am working towards it.

**Lichess**: Around **2150-2200** rated in Bullet and Rapid. Normally around **2100+** Blitz, for some reason

## Contributing

If you have any ideas, feel free to open an issue or even a pull request.
