import jedi
import orjson
from sys import stdout
from jedi import api


class Dumper:
    @staticmethod
    def dump_base_name(bn: api.classes.BaseName):
        return {
            "module_path": str(bn.module_path),
            "name": bn.name,
            "type": bn.type,
            "module_name": bn.module_name,
            "line": bn.line,
            "column": bn.column,
            "description": bn.description,
            "full_name": bn.full_name,
        }

    @staticmethod
    def dump_name(n: api.classes.Name):
        return Dumper.dump_base_name(n)

    @staticmethod
    def dump_completion(c: api.classes.Completion):
        base = Dumper.dump_base_name(c)
        base["complete"] = c.complete
        base["name_with_symbols"] = c.name_with_symbols
        base["type"] = c.type
        return base

    @staticmethod
    def dump_base_signature(bs: api.classes.BaseSignature):
        base = Dumper.dump_name(bs)
        base["params"] = [Dumper.dump_param_name(pn) for pn in bs.params]
        return base

    @staticmethod
    def dump_signature(s: api.classes.Signature):
        base = Dumper.dump_base_signature(s)
        base["index"] = s.index
        base["bracket_start"] = s.bracket_start
        return base

    @staticmethod
    def dump_param_name(pn: api.classes.ParamName):
        base = Dumper.dump_name(pn)
        # base["kind"] = pn.kind
        return base


def dump_base_names(base_names: list[api.classes.BaseName]):
    return [Dumper.dump_base_name(bn) for bn in base_names]


def dump_names(names: list[api.classes.Name]):
    return [Dumper.dump_name(n) for n in names]


def dump_completions(completions: list[api.classes.Completion]):
    return [Dumper.dump_completion(c) for c in completions]


def dump_base_signatures(base_signatures: list[api.classes.BaseSignature]):
    return [Dumper.dump_base_signature(bs) for bs in base_signatures]


def dump_signatures(signatures: list[api.classes.Signature]):
    return [Dumper.dump_signature(s) for s in signatures]


def dump_param_names(param_names: list[api.classes.ParamName]):
    return [Dumper.dump_param_name(pn) for pn in param_names]


def print_obj(obj):
    print(orjson.dumps(obj).decode("utf-8"), flush=True)


while True:
    exec(input())
