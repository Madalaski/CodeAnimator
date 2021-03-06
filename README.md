# CodeAnimator
 The Unity project I used to create Code Animations for my Tutorials

## Instructions For Use
To say that this project is in Beta, or even Alpha would be a gross overstatement. Really this was put together in two days for my Obra Dinn video, and whether or not I improve on it is really down to how much time I get outside of my dissertation and exams this year, but I will try because I think it could be a cool project worth persuing.

The project works by using the SampleScene which is setup to look like a Visual Studio-esque IDE. It will read whatever text is in the main Code Text box and then execture a list of "Changes" (which can be Line or Character specific in scope) in sequential order at a set speed, some having additional wait timers on them for animation.

The camera will move towards any line changes that happen outside of the central third of the screen.

Since programming the changes became tedious (and it uses 1-indexed line numbers at the moment the change is executed which requires some hardcore mathematical thinking) I created a scripting system.

It is *not* Turing complete. But it is simple enough that you should be able to take a script you have already written and easily program in the changes.

Each instruction is written at the beginning of the line with either "+++", "---" or "~~~" followed by the execution order (0-indexed) of that changes and a semicolon ";".

A "+++" command will add the following line to the code.

A "---" command will delete the following line from the code (but include it in the starting lines).

A "~~~" will replace the previous line with the following line, using string comparisons to make shortest path change. NOTE: This takes 2 instructions at the moment so make sure there are no instructions that have the same order index as the one after this instruction.

Store this in whatever kind of file you want (".txt" is probably best), pop the filename in the script and press the button at the bottom. Most likely cause for errors at the moment is you missing an instruction index somewhere. At the moment *has* to account for all instructions from 0 to your last one, though this will be the first thing I aim to change.

You can switch between the two formatting types I have ("CS" and "HLSL") by editing the code.

I've included examples from my Obra Dinn Shader tutorial so you can see how I pulled off the animations in that video!

## Current Problems / Future Improvements

This entire project exists so that it would save me time from manually recreating and animating the code I write in some Animation program. This only became true when I made the scripting system but it's incredibly volatile. Miss an index and it doesn't work. Realise you need to a Change to happen earlier, and if you have increment every instruction index that comes after it. It can become just as much a chore and just hand-animating so I need to figure out a system that makes it more streamlined. But it works well if you know exactly what you're doing first try.

Also the script is not particularly versatile when it comes to what changes you can make, even if the system supports it. For example with the script there is no way to delete a line you have added, unless you use "~~~" to change it an empty string. I joke about it not being Turing Complete, but ideally I would like it to be functionally sound enough that I don't have to worry too much about complex edge cases.

For texts larger than 100 lines, this gets very performance intensive. At first I thought it was from editing the textMesh itself so much, but I found out that it was actually because every time I added or deleted a character from the text via script, the TextMesh would regenerate *everything*. It slowed down on my 4-year-old laptop but my Desktop handled it fine. I will look for further (probably GPU-based) optimisations in the future.

"Change" is bad name for the struct. Will change the name and also change the struct into a Class so I can give that more functionality.

The text is currently formatted in a very inefficient way, so much so that I didn't put all the relevant syntax into my formatting functions just the bare minimum. I'll have to research syntax highlighting algorithms from other IDEs to solve this problem but I didn't ever see any noticeable slowdown from it.