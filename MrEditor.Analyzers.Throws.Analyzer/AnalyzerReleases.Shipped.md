## Release 0.1.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
TRW001  | Exceptions usage | Error | Method or property does not declare some exception types that may be thrown.
TRW002  | Exceptions usage | Error | Method or property declares [ThrowsNothing] but some exceptions may be thrown inside.
TRW901  | Exceptions usage | Info  | Catch clause rethrows non-CLS-exception (not supported by analyzer yet).
