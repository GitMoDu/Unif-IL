Unif-IL
=======

Unif-IL is a translation software to convert proprietary PLC code into IEC 61131-3 compliant IL code, using open XML dictionaries with a set of translation rules.

![](https://raw.githubusercontent.com/GitMoDu/Unif-IL/master/Media/UnifILscreenshot.jpg)

Motivation
=======
As the PLC automation industry moves forward to standard languages (as defined by the IEC 61131-3 standard), old programs from proprietary languages need to be developed again. Unless you use Unif-IL to automatically translate proprietary IL-style code into standard IL. If your preferred automation language is not included, just follow some simple rules and write a custom dictionary for a repeatable translation.

Context
=======
This work is based on the Master's thesis of Andre Caldeira Pereira, which is available in full (but only in Portuguese) at FCT-UNL repository: http://run.unl.pt/bitstream/10362/6871/1/Pereira_2011.pdf

The work in the thesis also provides another software tool, conversion software to simulate PLC control programs in the Matlab/Simulink environment, taking advantage of the IEC 61131 standard, and is available at https://github.com/GitMoDu/Matlaber .

Dependencies
Matlaber runs on pure .NET 3.5 , mainly for the interface and string handling functions. It's porting to other languages should be hassle free. 
