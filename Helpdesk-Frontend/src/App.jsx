import { BrowserRouter } from "react-router-dom";
import AppRoutes from "./routes/AppRoutes";
import AutenticacaoProvider from "./contexts/AutenticacaoProvider";
import "./App.css";

function App() {
    return (
        <BrowserRouter>
            <AutenticacaoProvider>
                <AppRoutes />
            </AutenticacaoProvider>
        </BrowserRouter>
    );
}

export default App;
