I stumbled onto a pretty valuable workflow today. One of the things my team has been
struggling with is the velocity of AI-assisted development. A lot of what we build are
smaller internal utilities, so we don't work on large legacy codebases that often —
which gives us a certain flexibility when it comes to leaning on AI to generate a decent
chunk of code.

Generally, I'm pro-AI. I mean, I'm using it heavily in this project. But there are real
downfalls. The big one: it's very difficult to review AI-generated code — not just because
of quality (though that's still an issue), but because of the sheer volume of it.

So the solution I landed on is what I'm uninspiredly calling *vibe reviewing*. The idea
is simple: describe your high-level thoughts about a PR to an agent and have it generate
the actual comments. In practice, I built a skill called `pair-pr-reviewing` that goes a
bit deeper into the process. I'll share some version of it soon, but I've already
identified two key principles:

- The discussion must be driven by the human reviewer. The agent helps with the details of the comments — not the topics. That part stays with you.
- The agent should push back when necessary. If your comment doesn't hold up, it should say so.

Early testing has gone really well. I got through two 1,000+ LOC PRs in about 10 minutes,
and the comments were solid. The agent pushed back on a few of mine. There was one where I
vaguely remembered some part of a framework that might clean up the code — it actually
went and checked whether the reference was valid before letting me run with it.

The skill also does more than just guide the review process. While building it, I gave it
clear instructions on how to format comments and what metadata to include — severity,
category, reviewer confidence — so the agent can take my stream-of-consciousness vibe of
a PR and turn it into something well-structured and legible.

## what jumped out

First, this reintroduces some much-needed friction into the dev process. The speed of AI
is great, but I'm a firm believer that these tools should elevate quality, not just
velocity. I think this process will help raise the baseline of the codebase over time —
it brings in a level of architectural review that AI review tools just aren't capable of yet.

Second — and I can't fully explain this — people seem more willing to accept comments
that feel like they came from AI. When feedback comes from a human, people tend to take
it personally, even if the code itself was AI-generated. That dynamic is interesting, but
it cuts both ways: the same openness that makes AI comments land more smoothly also makes
them easier to dismiss.
