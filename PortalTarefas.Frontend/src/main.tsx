import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { createBrowserRouter, Navigate, RouterProvider } from "react-router";
import App from "./App";
import ListaTarefas from "./paginas/ListaTarefas";
import DetalhesTarefa from "./paginas/DetalhesTarefa";
import NovaTarefa from "./paginas/NovaTarefa";
import Login from "./paginas/Login";
import ProvedorAutenticacao from "./autenticacao/ProvedorAutenticacao";
import RotaProtegida from "./autenticacao/RotaProtegida";
import "./index.css";

const roteador = createBrowserRouter([
  {
    path: "/",
    element: <App />,
    children: [
      { index: true, element: <Navigate to="/tarefas" replace /> },
      { path: "login", element: <Login /> },                       // pública
      {
        element: <RotaProtegida />,                                // daqui para baixo, só logado
        children: [
          { path: "tarefas", element: <ListaTarefas /> },
          { path: "tarefas/nova", element: <NovaTarefa /> },
          { path: "tarefas/:id", element: <DetalhesTarefa /> },
        ],
      },
      { path: "*", element: <p>Página não encontrada.</p> },
    ],
  },
]);

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <ProvedorAutenticacao>
      <RouterProvider router={roteador} />
    </ProvedorAutenticacao>
  </StrictMode>,
);