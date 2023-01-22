import jedi
from sys import stdout
import orjson

while True:

    file_path = input()
    file_lines = int(input())
    
    file_content = "\r\n".join([input() for _ in range(file_lines)])

    print(
        orjson.dumps(
            [
                {
                    "name": name.name,
                    "line": name.line,
                    "column": name.column,
                    "type": name.type,
                }
                for name in jedi.Script(file_content, path=file_path).get_names(
                    all_scopes=True, definitions=True, references=True
                )
            ]
        ).decode("utf-8")
    )
    stdout.flush()

