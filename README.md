CSharpKDMDiscoverer
===================

C# 4.0 preprocessor, lexer, and parser in Java.

The used ANTLR-based C# grammar is based on the one available at https://github.com/ChristianWulf/CSharpGrammar. I adapt it to not only parse a C# source file but also to build an abstract syntax tree from it.

License
---
Eclipse Public License - v 1.0 (http://www.eclipse.org/legal/epl-v10.html)

Example
---
A first example can be found in src/example/TransformationTest.java.

Compilation
---

#### Requirements

- JDK 1.5+
- Eclipse
- EMF
- JUnit

#### Steps to run

- If some EMF libraries are missing, let them be searched and added automatically by Eclipse via

~~~
Build Path -> Add Libraries... -> Plugin-in Dependencies -> Next -> Finish
~~~
