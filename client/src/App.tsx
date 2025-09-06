import { Route, Switch } from "wouter";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { Toaster } from "@/components/ui/toaster";
import { ThemeProvider } from "@/components/providers/ThemeProvider";
import { AuthProvider } from "@/contexts/AuthContext";
import Navigation from "@/components/layout/Navigation";
import Home from "@/pages/Home";
import Problems from "@/pages/Problems";
import IDE from "@/pages/IDE";
import Admin from "@/pages/Admin";
import Subscribe from "@/pages/Subscribe";
import Profile from "@/pages/Profile";
import Settings from "@/pages/Settings";
import ForgotPassword from "@/pages/ForgotPassword";
import ResetPassword from "@/pages/ResetPassword";
import NotFound from "@/pages/not-found";
import { ProtectedRoute } from "@/components/auth/ProtectedRoute";

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 5, // 5 minutes
      refetchOnWindowFocus: false,
    },
  },
});

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider defaultTheme="dark" storageKey="dsagrind-theme">
        <AuthProvider>
          <div className="min-h-screen bg-background text-foreground">
            <Navigation />
            <main className="flex-1">
              <Switch>
                <Route path="/" component={Home} />
                <Route path="/problems" component={Problems} />
                <Route path="/problems/:categorySlug" component={Problems} />
                <Route path="/problem/:slug">
                  <ProtectedRoute>
                    <IDE />
                  </ProtectedRoute>
                </Route>
                <Route path="/admin">
                  <ProtectedRoute requireAdmin>
                    <Admin />
                  </ProtectedRoute>
                </Route>
                <Route path="/subscribe">
                  <ProtectedRoute>
                    <Subscribe />
                  </ProtectedRoute>
                </Route>
                <Route path="/profile">
                  <ProtectedRoute>
                    <Profile />
                  </ProtectedRoute>
                </Route>
                <Route path="/settings">
                  <ProtectedRoute>
                    <Settings />
                  </ProtectedRoute>
                </Route>
                <Route path="/forgot-password" component={ForgotPassword} />
                <Route path="/reset-password" component={ResetPassword} />
                <Route component={NotFound} />
              </Switch>
            </main>
            <Toaster />
          </div>
        </AuthProvider>
      </ThemeProvider>
    </QueryClientProvider>
  );
}

export default App;