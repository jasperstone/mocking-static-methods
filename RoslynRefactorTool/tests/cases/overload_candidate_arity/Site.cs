using System;

namespace Acme;

// Receiver with several same-named overloads of DIFFERENT arity, mirroring the
// Microsoft.Extensions.Logging LogDebug family: a string-first SHORT overload vs
// an EventId/Exception LONG overload. This is the shape behind the orleans:0116
// overload-misselection bug.
public sealed class Diag
{
    // LONG sibling declared FIRST so a blind CandidateSymbols.FirstOrDefault()
    // would (wrongly) select it.
    public void Note(int eventId, Exception error, string message, params object[] args) { }

    // SHORT intended overload — the call below matches here by arity + type.
    public void Note(string message, params object[] args) { }
}

public sealed class OverloadWorker
{
    private readonly Diag _diag;

    public OverloadWorker(Diag diag)
    {
        _diag = diag;
    }

    public void Run(string state)
    {
        // The trailing argument is intentionally unbound so overload resolution
        // does NOT fully bind (symInfo.Symbol == null) and the tool must fall
        // back to CandidateSymbols — exactly the path the orleans net9/net10
        // reference split exercises. PickBestCandidate must select the SHORT
        // (string-first) overload by arity + argument type, NOT the first-listed
        // LONG one.
        _diag.Note("status {State}", state, __unbound_marker__);
    }
}
