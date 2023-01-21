import jedi
from sys import stdout
import orjson

while True:
    print(
        orjson.dumps(
            [
                {
                    "name": name.name,
                    "line": name.line,
                    "column": name.column,
                    "type": name.type,
                }
                for name in jedi.Script(path=input()).get_names(
                    all_scopes=True, definitions=True, references=True
                )
            ]
        ).decode("utf-8")
    )
    stdout.flush()
