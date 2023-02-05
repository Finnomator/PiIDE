# https://jedi.readthedocs.io/en/latest/docs/api-classes.html#completion
import jedi
from completion_dumper import (
    print_completions_with_type_hints,
    print_completions_without_type_hints,
)

while True:

    file_path = input()

    enable_type_hints = bool(int(input()))

    row = int(input())
    col = int(input())

    file_lines = int(input())

    file_content = "\r\n".join([input() for _ in range(file_lines)])

    script = jedi.Script(file_content, path=file_path)

    completions = script.complete(row, col)

    if enable_type_hints:
        print_completions_with_type_hints(completions)
    else:
        print_completions_without_type_hints(completions)
    break
class E:
    pass



def a():
    pass

b = "a"

