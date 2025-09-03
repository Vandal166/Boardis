import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import './index.css';
import { KeycloakProvider } from './KeycloakProvider.tsx';
import App from './App.tsx';
import Home from './pages/Home.tsx';
import BoardView from './pages/BoardView.tsx';
import Settings from './pages/Settings.tsx';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <KeycloakProvider>
      <BrowserRouter>
        <Routes>
          <Route element={<App />}>
            <Route path="/" element={<Home />} />
            <Route path="/board/:boardId" element={<BoardView />} />
            <Route path="/settings" element={<Settings />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </KeycloakProvider>
  </StrictMode>,
);