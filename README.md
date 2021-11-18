# Euchre
Two versions of my Euchre game -- the original VB/WinForms implementation, and the later C#/WPF implementation

## Background
Starting around 2006 (when I became the Visual Basic development manager) and going though about 2011, I wrote up a series of blogs on the Microsoft Visual Basic site that were focused on 
teaching people some of the basics of writing in VB.  I later posted these in an MSDN site called "The Temple of VB."  Both of these sites have,
alas, succumbed to bit rot, though several of the blogs (including the ones covering the material in this codebase) were captured in my e-book "The Temple of VB" which is available on Amazon.  (I know,
that's a shameless plug.)  The very first series of posts dealt with writing a Euchre game in Visual Basic, Euchre being a particular passion of mine in college.  I later ported this game to Windows Mobile 6.0 as well -- it was basically identical to the desktop except for having to modify the sizes of the resources somewhat.

That original Euchre game was written using WinForms, and as a result it was heavily focused on the "DoEvent" functionality to keep the game moving.  As a proof of concept, 
it was OK, but realistically that's not a very efficient way to run a game, because it locks the UI thread whenever waiting on a specific user response. I'd always
wanted to rewrite it to be more efficient.  I got my chance in 2013 when I took over as development manager of the Roslyn team.  I wanted to exercise Roslyn a bit,
and so I took the opportunity to convert the game to C# (a language I was much less familiar with at the time), and also rewriting it to WPF, as I was also curious 
about that UI stack.  The biggest change, though, was that I changed the entire architecture to be state-model based.  So, instead of blocking on the user for input, 
I simply cached the current state and let the message loop carryout whatever business it needed to do (including quitting/closing, something that was a lot trickier to handle in 
the VB version in that case), and then whenever the user clicked something on the screen, I'd figure out what state we were in and handle the event accordingly.

Time has marched on, and even my later C#/WPF implementation is looking a little, um, _quaint_.  If I were rewriting this app now, I'd certainly make this a mobile app and (in the 
spirit of .NET dogfooding) leverage something like MAUI to handle the UI stack.  MAUI and WPF are similar in the way that they handle UI elements and both are XAML-based, so the resource 
and event handling wouldn't be too much of a change, but the graphics themselves would certainly need an overhaul.  I'd also probably include a way to play with friends online.
The AI & rules logic in this code still holds up well, at least! 

Alas, I don't currently have the time to dig into another version, but as at least one person has recently expressed an interest in revisiting the code, I'm putting both the VB and C# versions
up on GitHub in case anyone else wants to play around with it.  I'm going to leave the main branch intact as the nominal baseline (mostly because I am unlikely to find time
to review PRs); changes should go into branches instead.

As far as the legal stuff goes (drum roll for the MIT license, please):

Copyright 2006, 2013, 2021 Matthew W. Gertz

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

