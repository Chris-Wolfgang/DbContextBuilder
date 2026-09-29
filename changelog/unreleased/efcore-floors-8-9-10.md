type: fix

`-Core-EF8`, `-Core-EF9` and `-Core-EF10` now require at least EF Core 8.0.31, 9.0.20 and 10.0.12 (up from 8.0.25, 9.0.14 and 10.0.5). A project that doesn't reference EF Core directly now restores the latest servicing patch. Upper bounds are unchanged, so each package still stays on its own EF Core major.
