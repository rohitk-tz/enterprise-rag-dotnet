# Building Enterprise RAG Applications with .NET

Companion material for the talk *Building Enterprise RAG Applications with .NET*.

| Path | Contents |
|---|---|
| [`Building_Enterprise_RAG_Applications_with_.NET.pptx`](Building_Enterprise_RAG_Applications_with_.NET.pptx) | Slide deck |
| [`Code/`](Code/) | Full-stack RAG platform: .NET 10 API + Workers, Python parsing service, Next.js frontend — see [Code/README.md](Code/README.md) |
| [`Quiz/`](Quiz/) | Audience quiz — open `Quiz/index.html` in a browser |

## Running the code

```bash
cd Code
cp .env.example .env                  # fill in your own keys
cp frontend/.env.example frontend/.env
docker compose up --build
```

See [Code/README.md](Code/README.md) for architecture and configuration details.
