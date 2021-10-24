# Setup & Run for development

- Download and setup [ngrok](https://ngrok.com/download)
- Lounch **WebApi** at _https://localhost:5001;http://localhost:5000_
- Run **ngrok** using next command:

```
ngrok http -region=eu -host-header=rewrite https://localhost:5001/
```
