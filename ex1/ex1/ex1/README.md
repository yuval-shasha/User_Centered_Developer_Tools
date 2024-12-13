# Submitters info

Regev Avraham 207708603 regev.avraham@campus.technion.ac.il
Yuval Shasha 315405662 yuval.shasha@campus.technion.ac.il

# Underspecified behaviors

### Should we support string localisation?
We decided to add localisation in order to create a tool that 
would be the easier to use, and more accessible for hebrew speakers.

### What is considered a global constant
Since in C# there is not an option for a global variable (all variables must be
under a class's scope), we decided to acknowledge public constant fields and public
static readonly fields as global variables.

### Which new name to suggest
We approached this error, by the logic that if a specific identifier isn't in a 
camel case, then it must be in a snake case and vise versa. We assumed that because 
we think that everyone write code with some convention, meaning there is a consistent
way for naming.


### Which rule category to choose
We chose that our linter will issue a warning in the naming category,
because our linter warn about naming conventions.

### Where to suggest a correction - each instance or only declaration
We suggest a correction only at the declaration of a variable. That is because 
the declaration is the first appearance of the variable's name and as such it is 
the representing instance. Moreover, we think that if the linter will paint
each instance of the variable with a warning color, it might annoy the user, 
and the user will quit using our linter. Also, after accepting the correction, all 
instances of the variable are changed.

### Which message to display on a grammar error or a typo
We chose to display a general error message for a typo, 
since we don't know exactly what the user was trying to write.

### which diagnostic identifier to show on a grammar error
Since the grammar diagnostic is not fixable using our tools, we used a different identifier from 
the convention error, so that the IDE won't suggest the convention fix on a grammar error.
We chose the id CS236651_Grammar, to keep the course number convention, but to supply extra information
about the error type.


### What is considered as local variable
we considered a local variable as a variable that was declared inside a function body.
We didn't apply the local variable rule on a function arguments or a field members. 
That is because we thought that the user may want a different syntax convention for a 
function's arguments (some IDEs even paint the argument in grey color) or other
convention for field members (like m_<id> or _<id>).

### Should we analyze generated code?
Generated code is a code that is generated automatically by some tool, and most of the
time doesn't get read by humans. Moreover, changing the already generated code is 
useless, since it will be generated automatically each time.

### Should work in concurrent execution
The linter should work in concurrent execution because we want it to find all 
occurrences of variable names that don't conform with the convention and not show
only one highlight at a time.
It was also what was written initially in the Roslyn's analyzer skeleton. 
