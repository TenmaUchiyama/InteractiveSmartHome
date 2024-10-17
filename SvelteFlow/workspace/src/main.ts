import App from "./App.svelte";
import "@/utils/SocketConnector";
import "./app.css";

const app = new App({
  target: document.getElementById("app")!,
});

export default app;
