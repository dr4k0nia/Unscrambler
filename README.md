# Unscrambler
**Universal unpacker and fixer for a number of modded ConfuserEx protections**

Dealing with simple modded protections like sizeof() mutations can be annoying. Unscrambler is meant to be used as an addition to de4dot fixing a number of protections not supported by de4dot.

## Features
Unscrambler can remove/fix the following:
- System.Math Mutations*
- Double.Parse() Mutations*
- Sizeof Mutations
- EmptyType Mutations
- Convert.ToInt32(Double) Mutations (only basic support)
- HideCalls
- Anti de4dot (using interface loop)
- Locals To Fields

> Due to the use of reflection these can fail if your target apps has a different framework, a fix is in the works.

## Usage
Either Drag&Drop your file onto unscrambler or use command line arguments like shown below:

       Unscrambler.exe <file>

## Credits / Thanks to
-  [AnonymooseRE](https://github.com/anonymoosere) For helping out with Unscrambler and answering a lot of my questions
-  [Washi](https://github.com/Washi1337/AsmResolver) For AsmResolver and answering my questions
- A ton of public ConfuserEx forks used as reference material
