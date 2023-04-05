import io
from orjson import dumps
from tokenize import tokenize

# from token import tok_name

while True:
    with open(input(), "rb") as f:
        print(
            dumps(
                [
                    {
                        "type": token.type,
                        # "type1": tok_name[token.type],
                        "exact_type": token.exact_type,
                        # "exact_type1": tok_name[token.exact_type],
                        "string": token.string,
                        # "line": token.line,
                        "start": token.start,
                        "end": token.end,
                    }
                    for token in tokenize(f.readline)
                ]
            ).decode("utf-8"),
            flush=True,
        )
