import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import './index.css';
import { KeycloakProvider } from './KeycloakProvider.tsx';
import App from './App.tsx';
import Home from './pages/Home.tsx';
import BoardView from './pages/BoardView.tsx';
import Settings from './pages/Settings.tsx';
import Boards from './pages/Boards.tsx';
import Dashboard from './pages/Dashboard.tsx';
import { ConfirmationDialogProvider } from './components/ConfirmationDialog.tsx';
import { SignalRProvider } from './communication/globalNotificationContex.tsx';
import { BoardSignalRProvider } from './communication/BoardSignalRProvider.tsx';
import './utils/18n/index.tsx';

const PlaceholderPage = ({ title }: { title: string }) => (
  <div className="container mx-auto p-6">
    <h1 className="text-3xl font-bold">{title}</h1>
    <p>This page is under construction.</p>
  </div>
);

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <KeycloakProvider>
      <SignalRProvider>
        <BoardSignalRProvider>
          <ConfirmationDialogProvider>
            <BrowserRouter>
              <Routes>
                <Route element={<App />}>
                  <Route path="/" element={<Home />} />
                  {/* Dashboard layout with nested routes */}
                  <Route path="/dashboard" element={<Dashboard />}>
                    <Route index element={<Boards />} />
                    <Route path="board/:boardId" element={<BoardView />} />
                  </Route>
                  <Route path="/settings" element={<Settings />} />
                  <Route path="/profile" element={<PlaceholderPage title="Your Profile" />} />
                  <Route path="/features" element={<PlaceholderPage title="Features" />} />
                  <Route path="/pricing" element={<PlaceholderPage title="Pricing" />} />
                  <Route path="/blog" element={<PlaceholderPage title="Blog" />} />
                  <Route path="/help" element={<PlaceholderPage title="Help Center" />} />
                  <Route path="/contact" element={<PlaceholderPage title="Contact Us" />} />
                  <Route path="/privacy" element={<PlaceholderPage title="Privacy Policy" />} />
                  <Route path="/terms" element={<PlaceholderPage title="Terms of Service" />} />
                  <Route path="*" element={<PlaceholderPage title="Not Found" />} />
                </Route>
              </Routes>
            </BrowserRouter>
          </ConfirmationDialogProvider>
        </BoardSignalRProvider>
      </SignalRProvider>
    </KeycloakProvider>
  </StrictMode>,
);