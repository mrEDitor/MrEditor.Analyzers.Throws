Exception checking for .NET
===========================

TODO
====

- Research non-CLS-complaint calls and their throwings.
- Research BeginInvoke/EndInvoke calls.
- Research dynamic-related invocations.
- Research smarter contract that allows method to accept delegate and call it in-place,
  possibly resulting in throwing of type not declared by method signature, but declared
  for actual parameter.
- Research exhaustive catch clauses which may result in rethrowing; do not escalate
  throwing type to one of catch clause, but use set of such of try block.
- Implement external attributes to mark built-in and third-party libraries with.

See also
========

- https://habr.com/ru/post/253833/ (code at https://github.com/SergeyTeplyakov/ExceptionAnalyzer);
- https://cezarypiatek.github.io/post/exceptions-usages-analyzer/ (code at https://github.com/smartanalyzers/ExceptionAnalyzer).
