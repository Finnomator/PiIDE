from orjson import dumps
from sys import stdout
from jedi.api.classes import Completion, Name


def print_completions_without_type_hints(completions: list[Completion]):
    print(
        dumps(
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
                    "column": completion.column,
                    "type_hint": None,
                }
                for completion in completions
            ]
        ).decode("utf-8")
    )
    stdout.flush()


def print_completions_with_type_hints(completions: list[Completion]):
    data = []
    for completion in completions:
        try:
            type_hint = completion.get_type_hint()
        except TypeError:
            type_hint = None
        data.append(
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
                "column": completion.column,
                "type_hint": type_hint,
            }
        )
    print(dumps(data).decode("utf-8"))
    stdout.flush()


def print_names_with_type_hints(names: list[Name]):
    data = []
    for name in names:
        try:
            type_hint = name.get_type_hint()
        except TypeError:
            type_hint = None
        data.append(
            {
                "name": name.name,
                "description": name.description,
                "docstring": name.docstring(),
                "is_keyword": name.is_keyword,
                "module_name": name.module_name,
                "module_path": str(name.module_path),
                "type": name.type,
                "line": name.line,
                "column": name.column,
                "type_hint": type_hint,
            }
        )
    print(dumps(data).decode("utf-8"))
    stdout.flush()


def print_names_without_type_hints(names: list[Name]):
    print(
        dumps(
            [
                {
                    "name": name.name,
                    "description": name.description,
                    "docstring": name.docstring(),
                    "is_keyword": name.is_keyword,
                    "module_name": name.module_name,
                    "module_path": str(name.module_path),
                    "type": name.type,
                    "line": name.line,
                    "column": name.column,
                    "type_hint": None,
                }
                for name in names
            ]
        ).decode("utf-8")
    )

    stdout.flush()
