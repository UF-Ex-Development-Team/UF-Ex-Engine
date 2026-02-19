# C# Crash Course

Since UF-Ex uses C#, it is expected that you have prior knowledge of C# to use this. This section will quickly go over commonly used elements in chart making for absolute beginners.

## Variables
Variables help you store data, you can define a variable like in these formats:

```
<type> <name>;
<type> <name> = <value>;
<type> <name> = <value>, <name>;
<type> <name> = <value>, <name> = <value>;
```

The most commonly used variable types are `int` (Integers), `float` (Number with a decimal), `char` (Single character), `string` (Text), and `bool` (Boolean, meaning it's either True or False).

For example:

```csharp
int integer = 0;
float number = 9.2f; //Note that you need to put "f" at the end of the number to indicate it is a float
char character = 'A'; //Single quotation marks for char
string text = "Hello World!"; //Double quotation marks for string
bool condition_true = true, condition_false = false;
int some_number; //This will mark "some_number" as an integer, but it will not have a value assigned to it
```

## Operations
Code, like Maths, have operation symbols `+` (Add), `-` (Subtract), `*` (Multiply), `/` (Divide).

```csharp
int a = 5 + 6; //Which is 5 + 6 = 11
int b = a * 6; //Which is 11 * 6 = 66
string text_start = "Hello";
string text_end = " World!";
string full_text = text_start + text_end; //Which is "Hello World!"
```

Keep in mind that sometimes, two different types can be added together, but sometimes it cannot:

```csharp
string something = "Text" + 5; //This will become "Text5"

var something_else = 5 + true; //This will result in an error

string long_text = "text a" * "text b"; //This will result in an error
```

There are also these operators called **compound assignments**, it makes operating easier:

```csharp
int a = 5;
//Instead of this
a = a + 6;
//You can do this
a += 6;
```

In cases where you want to add or subtract 1, you can use `++` and `--` to make it even shorter:

```csharp
int a = 5;
a = a + 1; //It becomes 6
a += 1; //It becomes 7
a++; //It becomes 8
a--; //It becomes 7
```

## Collections

In some occasions, you might want to store a lot of variables of the same type together, such as:

```csharp
int value_1 = 1;
int value_2 = 2;
int value_3 = 3;
int value_4 = 4;
int value_5 = 5;
int value_6 = 6;
int value_7 = 7;
```

Instead of this, you can use collections like **Arrays**:

```csharp
int[] values = new int[7]; //This specifies the array to have the length 7
values[0] = 1; //This assigns the first value of the array to be 1
values[1] = 2; //This assigns the second value of the array to be 2
```
or
```csharp
int[] values = [1, 2, 3, 4, 5, 6, 7]; //Assigns the length of the array and all the values in it
```

You can easily store and retrieve data stored in an array. However, you cannot change the size of an array once you initialized it. If you want a collection where its size can be changed dynamically, you should use a `List`:

```csharp
List<int> values = [1, 2, 3, 4, 5, 6]; //Assigns the length of the list, type of the list, and the initial values in it
values.Add(7); //Adds "7" to the end of the list, increasing its length
//The list will become [1, 2, 3, 4, 5, 6, 7]
values.RemoveAt(0); //Removes the first element, in this case "1", from the list
//The list will become [2, 3, 4, 5, 6, 7]
```

Similiar to arrays, you can read or write values by using `values[<index>]`.

### Shorthand initialization

Sometimes, the variable name might be longer, such as:

```csharp
EaseUnit<Vector2>[] Eases = new EaseUnit<Vector2>[20];
```

If you don't want to type the whole variable type out, you can do this instead

```csharp
var Eases = new EaseUnit<Vector2>[20];
```

It will automatically assign `Eases` to the defined variable type if it is not ambiguous between multiple variable types.

```csharp
var Eases = new EaseUnit<Vector2>[20]; //Clear definition of EaseUnit<Vector2>[], no issues

List<EaseUnit<Vector2>> Eases = []; //Clear definition of List<EastUnit<Vector2>>, no issues

EaseUnit<Vector2>[] Eases = []; //Clear definition of EaseUnit<Vector2>[], no issues

var Eases = []; //Ambiguous between EaseUnit<Vector2>[], List<EastUnit<Vector2>>, etc., will result in error
```

## Statements

One of the most important aspects of coding is statements, it's rather straight forward actually. There are some commonly used operators assosiated with it, being `==` (Equal), `!=` (Not equal), `>` (Greater than), `>=` (Greater than or equal), `<` (Less than), `<=` (Less than or equal).

```csharp
int a = 5;
if (a < 10) //Since a is less than 10, this part will run
{
    //Code
}
else //Since the part above was already executed, this part will not run
{
    //Code
}
```

Similarly

```csharp
int a = 5;
if (a > 10) //Since a is not greater than 10, this part will not run
{
    //Code
}
else //Since the condition above is not true, this part will run
{
    //Code
}
```

In some cases, you would like to have a variable that has its value depending on the value of another variable, like:

```csharp
int a = 0, b;
if (a == 0)
    b = 1;
else if (a == 1)
    b = 3;
else if (a == 2)
    b = 9;
else
    b = 12;
```

You may notice that this can quickly become a bit messy, in this case, you can use **switch** statements:

```csharp
switch (a)
{
    case 0: //When a is 0, execute the following code
        b = 1;
        break; //Indicates the code ends here
    case 1: //When a is 1, execute the following code
        b = 3;
        break; //Indicates the code ends here
    case 2: //When a is 2, execute the following code
        b = 9;
        break; //Indicates the code ends here
    default: //When a is none of the above, execute the following code
        b = 12;
        break; //Indicates the code ends here
}
```

or

```csharp
b = a switch
{
    0 => 1, //When a is 0, b is 1
    1 => 3, //When a is 1, b is 3
    2 => 9, //When a is 2, b is 9
    _ => 12 //When a is none of the above, b is 12
};
```

In some cases, you may run into this type of code:

```csharp
int a = 0, b;
if (a == 0)
    b = 1;
else if (a == 1)
    b = 1;
else if (a == 2)
    b = 9;
else
    b = 12;
```

You can do something like this:

```csharp
switch (a)
{
    case 0:
    case 1:
        b = 1; //When a is 1 or 0, execute this
        break;
    case 2: //Same as before
        b = 9;
        break;
    default:
        b = 12;
        break;
}

//Or this

b = a switch
{
    0 or 1 => 3, //When a is 0 or 1, b is 3
    2 => 9, //When a is 2, b is 9
    _ => 12 //When a is none of the above, b is 12
};
```

## Functions
Functions help you perform repetitive tasks, you can define a function like this:

```csharp
<return_type> <name>(<type> <name>, <type> <name>, ...)
{
    //Code
    return Some_value; //If necessary
}
```

For example instead of the following:

```csharp
int a = 5, b = 6;
float c = (a + b) / 2f;
```

You can do this instead:

```csharp
float Mean(int value_a, int value_b)
{
    return (value_a + value_b) / 2f;
}

int a = 5, b = 6;
float c = Mean(a, b);
```

If the function only contains one line of code, like the one above, you can use an expression called **lambda expression** instead:

```csharp
float Mean(int value_a, int value_b) => (value_a + value_b) / 2f;
```

Of course, you can define a function that does not return anything:

```csharp
void SomeFunction()
{
    //Code
    //In this case, you do not need to add "return" as there is nothing to return
}
```

This also means that you cannot do something like this:

```csharp
float c = SomeFunction();
```

## Loops
Let's say you have an array:

```csharp
int[] values = [1, 2, 3, 4, 5, 6, 7];
```

And you want all the values to increase by 1, so that it will become `[2, 3, 4, 5, 6, 7, 8]`, while you can manually add them like this:

```csharp
values[0]++;
values[1]++;
values[2]++;
values[3]++;
values[4]++;
values[5]++;
values[6]++;
values[7]++;
```

You'd probably wonder if there are any better ways to handle this, and there are! They are loops, one of them are **while loops**, you can use them like this:

```csharp
//Defines i as 0
int i = 0;
//Checks whether i is less than the length of the array, if so, run the code
while (i < value.Length)
{
    //Increases the i-th index of the array by 1
    values[i]++;
    //Increases i by 1
    i++;
    //Goes back to the check
}
//The loop ended
```

However, in cases like these, there is an even better loop for it, it's called **for loops**:

```csharp
for (<type> <name> = <value>; <condition>; <action>)
{
    //Code
}
```

For the above example, the example code would be this:

```csharp
for (int i = 0; i < values.Length; i++)
{
    values[i]++;
}
```

You may notice that it is functionally the same as the above while loop.

---

## Using statement

In UF-Ex, you are likely to use other namespaces or types to use their functions, for example:

```csharp
float radian = UndyneFight_Ex.MathUtil.GetRadian(100);
ICustomMotion motion = UndyneFight_Ex.Entities.Motions.RotationRoute.linear;
```

As you may see, it's tedious to type out "UndyneFight_Ex.XXX" every single time we have to reference it, so you can use `using` to simplify the process, the syntax is as follows:

```csharp
using (namespace name);
```

using the_above_example; (Please laugh)

```csharp
using UndyneFight_Ex;

float radian = MathUtil.GetRadian(100);
ICustomMotion motion = Entities.Motions.RotationRoute.linear;
```

Sometimes, you would want to simplify static types that are highlighted in green (in default), such as `MathUtil`. To simplify these static members, you can simply use
```csharp
using static UndyneFight_Ex.MathUtil;

float radian = GetRadian(100);
```

Note that this does not imply `UndyneFight_Ex` will be used, so you still need to type out `using UndyneFight_Ex;`.

---

And this is the basics of C# used in chart making.

> [!TIP]
> You can always search online for more information about C# to make coding easier, just make sure the codes don't get too messy \\^o^/.