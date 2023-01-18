# https://jedi.readthedocs.io/en/latest/docs/api-classes.html#completion

import orjson
import jedi
from os import system
from sys import stdout

while True:
    path = input()
    row = int(input())
    col = int(input())

    system("cls")

    file_content = open(path).read()

    script = jedi.Script(path=path)

    completions = script.complete(row, col)

    data = {}

    for completion in completions:
        data[completion.name] = {
            "complete": completion.complete,
            "name_with_symbols": completion.name_with_symbols,
            "description": completion.description,
            "docstring": completion.docstring(),
            "is_keyword": completion.is_keyword,
            "module_name": completion.module_name,
            "module_path": str(completion.module_path),
            "type": completion.type,
            "line": completion.line,
        }

    print(orjson.dumps(data).decode("utf-8"))
    stdout.flush()
