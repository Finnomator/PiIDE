# https://jedi.readthedocs.io/en/latest/docs/api-classes.html#completion

import orjson
import jedi
from sys import stdout

while True:

    file_path = input()

    row = int(input())
    col = int(input())

    file_lines = int(input())

    file_content = "\r\n".join([input() for _ in range(file_lines)])
    
    script = jedi.Script(file_content, path=file_path)

    completions = script.complete(row, col)

    print(
        orjson.dumps(
            [
                {
                    "name": completion.name,
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
                for completion in completions
            ]
        ).decode("utf-8")
    )

    stdout.flush()
