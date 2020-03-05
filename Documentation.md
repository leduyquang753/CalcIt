# Documentation
This page describes usage of CalcIt UWP.

## General
The program’s main window is where you enter your mathematical expressions into the *Input* box to be evaluated, either by pressing the *Calculate* button or hitting Enter. The program will (try to) evaluate and output its response to the *Console*. The *Console* retains all calculations performed during a session of the program unless you have pressed *Clear output*.

Any spaces in the expression will be ignored and everything is case-insensitive. You can use pipe characters to separate multiple expressions to calculate at a time.

Pressing *View variables* will pull out a pane where you can specify variables whose values are to display. Press the + button and type in the variable's name in the *Name* box to add a new variable.

Pressing *Settings* allows you to tweak the program's behavior.
Anything there is pretty straightforward to understand, so detailed documentation isn't needed here, except some notes:
- Changing *Maximum expressions stored* will trim out the history if it exceeds the new maximum.
- *Startup expressions* can have comments after double slashes on each line and doesn't allow pipe characters to separate expressions.

### Calculator behavior and syntax
#### Basics
An expression has two parts: variable assignment (optional) and the main expression to be evaluated.
- Variable assignment comprises of a variable name followed by an equal sign (=). Multiple variable assignments can be chained in the same expression.
- The main expression can contain numbers, variables, operators and functions.

*Examples:*
`16,3+27,5.3-3^2sin(45)` will evaluate to 90,7928932188.
`tables=racks=50:5` will evaluate to 10 and assign it to variables named `tables` and `racks`.

#### Numbers
A number entered into the program must follow these requirements:
- Uses a dot or comma to separate the integral and fractional part, cannot have more than one dot/comma.
- Can have thousand separators (dots, commas or spaces depending on the settings) between digits but not at the beginning.
- Can have negative (-) or positive (+) signs at the beginning. Note: The precedence of the negative and positive signs depends on the expression, see [Calculation order](#calculation-order).
- Can use a percent sign (%) at the end to divide the number by 100, cannot have more than one percent sign.

#### Variables
A variable stores a number which can be used inside an expression. Each variable is identified by a name that must follow these requirements:
- Can only contain a÷z, 0÷9 and underscore (_) characters.
- Cannot start with a digit.

#### Operands
An operand is placed between two numbers/variables to perform a mathematical operation on the numbers.

The program currently supports these operands:

\+	Addition

\-	Subtraction

. \*	Multiplication

: /	Division

^	Exponentiation

\#	Root

#### Braces and functions
A section of the expression can be grouped by surrounding with one of these character pairs: () [] \{\} <>, in which case the grouped expression is called a bracelet. The expression inside will be calculated first before its result is used for evaluation of the greater expression part. If an identifier (similar to a variable name) is placed right before the opening brace, the bracelet will become a function. A function will take the expression inside the bracelet as their arguments and evaluate to a number. A function can have multiple arguments, in which case the arguments are separated by semicolons (;).

Negative and positive signs can also be placed before a bracelet.

*Notes:*
- If there are more than one parameter inside a bracelet that is not a function, the bracelet will evaluate to the sum of the parameters.
- If a bracelet is empty, it will evaluate to 0. Similarly, if a parameter is empty, it will be treated as 0. The consequence is a function can never have 0 parameters.
- The precedence of the negative and positive signs depends on the expression, see [Calculation order](#calculation-order).

For a list of functions and how they should be used, refer to the [List of functions](#list-of-functions).

#### Multiplication without sign
In some places, the multiplication sign can be omitted in the expression, in which case the multiplication’s precedence will be raised higher than normal multiplication and division (see Calculation order). The places where the multiplication sign can be omitted are:

- Between a number before and a variable after, example: `13pi`.
- Between a number before and a bracelet after, example: `13(3+5)`.
- Between a bracelet before and a number/variable after, examples: `(3+5)13`; `(2,5+16,8)pi`.
- Between two bracelets, example: `(2+3)(4+5)`.

#### Calculation order
The program evaluates the expression with respect to mathematical precedence, as follows:

Order|Things that are evaluated
-----|-----
\#1 First|Bracelets
\#2 Second|Exponentiation (calculation order: right to left)
\#3 Third|Root (calculation order: left to right)
\#4 Fourth|Multiplication without sign (calculation order: left to right)
\#5 Fifth|Multiplication with sign, division (calculation order: left to right)
\#6 Sixth|Addition, subtraction (calculation order: left to right)

The negative/positive signs’ precedence is a bit special: it’s higher than operators before and lower than operators with higher precedence than multiplication, for example `2^-2` will calculate `-2` first before performing the exponentiation, while `-2^2` will perform 2^2 = 4 before the result gets negated.

### Output
If the expression is valid, after evaluating the full expression, the program will print to the console an equal sign (=) followed by the evaluated number, clearing the Input box. Otherwise, it will print `ERROR:` and explain what was wrong with the expression, the expression will be retained in the box. In some cases, the Input box’s caret will move to the erroneous place in the expression.

#### Special use cases
You can exploit the program’s abilities to do some awesome things. Here lists a few of them:

##### Convert units
For example you can calculate how many seconds is 17h01:25 through the use of variables and bracelets:

```
hours=hour=3600
= 3 600

minutes=minute=60
= 60

seconds=second=1
= 1

(17 hours; 1 minute; 25 seconds)
= 61 285
```

##### Calculating a simple series
For example the Fibonacci sequence can be computed as follows:

```
1
= 1

1
= 1

Ans+PreAns
= 2

Ans+PreAns // Repeatedly pressing Enter will repeatedly calculate the same expression.
= 3

Ans+PreAns
= 5

Ans+PreAns
= 8

Ans+PreAns
= 13
```

## List of functions
**sum(<number 1>[; <number 2>][; <number 3>][…])**

Calculates the total of the arguments, as if the arguments are surrounded by bracelets and addition signs are placed between.

*Aliases:* total

**floor(<number 1>[; <number 2>][; <number 3>][…])**

Calculates the integral part of the total of the arguments.

*Aliases:* flr

**round(<number 1>[; <number 2>][; <number 3>][…])**

Calculates the rounded total of the arguments. If the total’s absolute value’s fractional part is at least 0,5, it will be rounded to the nearest to 0 number which is farther to 0 than the total: 3,6 -> 4; -1,5 -> -2, otherwise it will be floored.

*Aliases:* rnd

**abs(<number 1>[; <number 2>][; <number 3>][…])**

Calculates the absolute value of the total of the arguments.

*Aliases:* absolute

**sin(<number 1>[; <number 2>][; <number 3>][…])**

Calculates the sine value of the sum of the arguments in degrees.

*Aliases:* sine

**cos(<number 1>[; <number 2>][; <number 3>][…])**

Calculates the cosine value of the sum of the arguments in degrees.

*Aliases:* cosine

**tan(<number 1>[; <number 2>][; <number 3>][…])**

Calculates the tangent value of the sum of the arguments in degrees.

*Aliases:* tg, tang, tangent

**cot(<number 1>[; <number 2>][; <number 3>][…])**

Calculates the cotangent value of the sum of the arguments.

*Aliases:* cotg, cotang, cotangent

**arcSin(<number 1>[; <number 2>][; <number 3>][…])**

Calculates the angle in degrees whose sine value is the sum of the arguments.

*Aliases:* arcSine, sin_1, sine_1

**arcCos(<number 1>[; <number 2>][; <number 3>][…])**

Calculates the angle in degrees whose cosine value is the sum of the arguments.

*Aliases:* arcCosine, cos_1, cosine_1

**arcTan(<number 1>[; <number 2>][; <number 3>][…])**

Calculates the angle in degrees whose tangent value is the sum of the arguments.

*Aliases:* arcTg, arcTang, arcTangent, tg_1, tan_1, tang_1, tangent_1

**arcCot(<number 1>[; <number 2>][; <number 3>][…])**

Calculates the angle in degrees whose cotangent value is the sum of the arguments.

*Aliases:* arcCotg, arcCotang, arcCotangent, cotg_1, cotan_1, cotang_1, cotangent_1

**log(<number 1>[; <number 2>][; <number 3>][…])**

If there is only one argument, calculates base-10 logarithm of the argument. The argument must be positive.

If there is more than one, calculates base-<number 1> logarithm of the sum of other arguments. <number 1> and the sum of the rest must be positive.

*Aliases:*  logarithm, logarid

**ln(<number 1>[; <number 2>][; <number 3>][…])**

Calculates natural (base-e¬) logarithm of the sum of the parameters, which must be positive.

*Aliases:* loge, natural_logarithm, natural_logarid

**GCD(<number 1>[; <number 2>][; <number 3>][…])**

Calculates the greatest common divisor of the arguments floored.

*Aliases:* greatestCommonDivisor, greatest_common_divisor

**LCM(<number 1>[; <number 2>][; <number 3>][…])**

Calculates the lowest positive common multiplier of the arguments floored.

*Aliases:* lowestCommonMultiplier, lowest_common_multiplier

**fact(<number 1>[; <number 2>][; <number 3>][…])**

Calculates the factorial of the sum of the arguments floored, which must not be negative.

*Aliases:* factorial

**P(\<n\>; \<k\>)**

Calculates the k-permutation of n elements. The arguments are floored and must not be negative.

*Aliases:* permutation

**C(\<n\>; \<k\>)**

Calculates the k-combination of n elements. The arguments are floored and must not be negative.

*Aliases:* combination

**max(<number 1>[; <number 2>][; <number 3>][…])**

Retrieves the argument with the highest value as the result.

*Aliases:* maximum

**min(<number 1>[; <number 2>][; <number 3>][…])**

Retrieves the argument with the lowest value as the result.

*Aliases:* minimum

**average(<number 1>[; <number 2>][; <number 3>][…])**

Retrieves the average value of the arguments.

*Aliases:* avg

**random(<number 1>[; <number 2>])**

If there is one argument, returns a random number between 0 and <number 1> inclusive.

If there is two, returns a random number between <number1> and <number 2> inclusive.

*Aliases:* rand

**randomInt(<number 1>[; <number 2>])**

If there is one argument, returns a random integer between 0 and <number 1> inclusive.

If there is two, returns a random integer between <number1> and <number 2> inclusive.

*Aliases:* randInt, randomInteger, random_int

**randomInList(<number 1>[; <number 2>][; <number 3>][…])**

Retrieves a random argument as the result.

*Aliases:* randInList, random_in_list, rand_in_list

**isGreater(<number 1>; <number 2>)**

Returns 1 if <number 1> is greater than <number 2>, 0 otherwise.

**isSmaller(<number 1>; <number 2>)**

Returns 1 if <number 1> is smaller than <number 2>, 0 otherwise.

**isEqual(<number 1>; <number 2>)**

Returns 1 if <number 1> is equal to <number 2>, 0 otherwise.

**and(<number 1>[; <number 2>][; <number 3>][…])**

Returns 1 if all arguments are greater than 0, 0 otherwise.

**or(<number 1>[; <number 2>][; <number 3>][…])**

Returns 1 if at least one of the arguments is greater than 0, 0 if all arguments are not greater than 0.

**not(\<number\>)**

Returns 0 if the argument is greater than 0, 1 otherwise.

**if(\<condition\>[; \<value if true\>][; \<value if false\>])**

Returns <value if true> if <condition> is greater than 0, <value if false> otherwise.

If <value if true>/<value if false> is not present, defaults to 0.

**degToRad(<number 1>[; <number 2>][; <number 3>][…])**

Converts the total value of the arguments from degrees to radians.

*Aliases:* degreesToRadians, deg_to_rad, degrees_to_radians

**degToGrad(<number 1>[; <number 2>][; <number 3>][…])**

Converts the total value of the arguments from degrees to gradians.

*Aliases:* degreesToGradians, deg_to_grad, degrees_to_gradians

**radToDeg(<number 1>[; <number 2>][; <number 3>][…])**

Converts the total value of the arguments from radians to degrees.

*Aliases:* radiansToDegrees, rad_to_deg, radians_to_degrees

**radToGrad(<number 1>[; <number 2>][; <number 3>][…])**

Converts the total value of the arguments from radians to gradians.

*Aliases:* radiansToGradians, rad_to_grad, radians_to_gradians

**gradToDeg(<number 1>[; <number 2>][; <number 3>][…])**

Converts the total value of the arguments from radians to degrees.

*Aliases:* gradiansToDegrees, grad_to_deg, gradians_to_degrees

**gradToRad(<number 1>[; <number 2>][; <number 3>][…])**

Converts the total value of the arguments from gradians to radians.

*Aliases:* gradiansToRadians, grad_to_rad, gradians_to_radians

## Source code and copyright
CalcIt UWP is open source here. You can read, download, modify, contribute under the terms of [the MIT license](https://github.com/leduyquang753/CalcIt-UWP/blob/master/LICENSE.txt).

## Problem detection and reporting
If the program crashes, you may report the crash as an issue in this repository.

If you believe the program’s calculation result is wrong, please do the following before submitting an issue:

- Check if the expression is entered correctly as you want it to be.
- Check if the operators’ precedence is correctly set in the expression.
- Perform the calculation on some other trustable calculator devices/programs and compare the results.

If you want to suggest a new feature of modification, feel free to do so by submitting an issue.