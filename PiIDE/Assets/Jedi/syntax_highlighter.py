import jedi
from completion_dumper import (
    print_names_with_type_hints,
    print_names_without_type_hints,
)

while True:

    file_path = input()
    enable_type_hints = bool(int(input()))
    file_lines = int(input())

    file_content = "\r\n".join([input() for _ in range(file_lines)])

    if enable_type_hints:
        print_names_with_type_hints(
            jedi.Script(file_content, path=file_path).get_names(
                all_scopes=True, definitions=True, references=True
            )
        )
    else:
        print_names_without_type_hints(
            jedi.Script(file_content, path=file_path).get_names(
                all_scopes=True, definitions=True, references=True
            )
        )
