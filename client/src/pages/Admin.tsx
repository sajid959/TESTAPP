import { useState } from 'react';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { ProtectedRoute } from '@/components/auth/ProtectedRoute';
import { AdminDashboard } from '@/components/admin/AdminDashboard';
import { ProblemManager } from '@/components/admin/ProblemManager';
import { CategoryConfig } from '@/components/admin/CategoryConfig';
import { BulkImport } from '@/components/admin/BulkImport';
import { useAuth } from '@/hooks/useAuth';

function AdminContent() {
  const { user, logout } = useAuth();
  const [activeTab, setActiveTab] = useState('dashboard');

  const tabs = [
    {
      id: 'dashboard',
      label: 'Dashboard',
      icon: 'fas fa-tachometer-alt',
      component: <AdminDashboard />,
    },
    {
      id: 'problems',
      label: 'Problems',
      icon: 'fas fa-code',
      component: <ProblemManager />,
    },
    {
      id: 'categories',
      label: 'Categories',
      icon: 'fas fa-folder',
      component: <CategoryConfig />,
    },
    {
      id: 'import',
      label: 'Bulk Import',
      icon: 'fas fa-upload',
      component: <BulkImport />,
    },
    {
      id: 'users',
      label: 'Users',
      icon: 'fas fa-users',
      component: <UserManagement />,
    },
    {
      id: 'analytics',
      label: 'Analytics',
      icon: 'fas fa-chart-bar',
      component: <Analytics />,
    },
  ];

  return (
    <div className="min-h-screen bg-slate-50 dark:bg-slate-900">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Header */}
        <div className="flex justify-between items-center mb-8">
          <div>
            <h1 className="text-3xl font-bold text-slate-900 dark:text-white mb-2">
              <i className="fas fa-shield-alt mr-3 text-brand-600"></i>
              Admin Dashboard
            </h1>
            <p className="text-slate-600 dark:text-slate-400">
              Manage platform settings, users, and content
            </p>
          </div>
          
          <div className="flex items-center space-x-4">
            <div className="flex items-center space-x-2">
              <Badge variant="default" className="bg-brand-600">
                <i className="fas fa-user-shield mr-1"></i>
                {user?.username}
              </Badge>
              <Badge variant="outline">Admin</Badge>
            </div>
            <Button 
              variant="outline" 
              onClick={logout}
              className="text-error-600 hover:text-error-700 hover:bg-error-50 dark:hover:bg-error-950"
              data-testid="button-admin-logout"
            >
              <i className="fas fa-sign-out-alt mr-2"></i>
              Logout
            </Button>
          </div>
        </div>

        {/* Main Content */}
        <Tabs value={activeTab} onValueChange={setActiveTab} className="space-y-6">
          {/* Tab Navigation */}
          <div className="bg-white dark:bg-slate-900 rounded-xl shadow-sm border border-slate-200 dark:border-slate-800 overflow-hidden">
            <TabsList className="grid grid-cols-6 w-full h-auto p-1 bg-transparent">
              {tabs.map((tab) => (
                <TabsTrigger
                  key={tab.id}
                  value={tab.id}
                  className="flex items-center justify-center p-4 text-sm font-medium data-[state=active]:bg-brand-50 data-[state=active]:text-brand-600 dark:data-[state=active]:bg-brand-950"
                  data-testid={`tab-${tab.id}`}
                >
                  <i className={`${tab.icon} mr-2`}></i>
                  {tab.label}
                </TabsTrigger>
              ))}
            </TabsList>
          </div>

          {/* Tab Content */}
          {tabs.map((tab) => (
            <TabsContent key={tab.id} value={tab.id} className="space-y-6">
              {tab.component}
            </TabsContent>
          ))}
        </Tabs>
      </div>
    </div>
  );
}

// User Management Component
function UserManagement() {
  const mockUsers = [
    { id: '1', username: 'alice_coder', email: 'alice@example.com', role: 'user', isPremium: true, joinDate: '2024-01-15' },
    { id: '2', username: 'bob_dev', email: 'bob@example.com', role: 'user', isPremium: false, joinDate: '2024-02-20' },
    { id: '3', username: 'charlie_admin', email: 'charlie@example.com', role: 'admin', isPremium: true, joinDate: '2023-12-01' },
  ];

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center justify-between">
          <span>
            <i className="fas fa-users mr-2"></i>
            User Management
          </span>
          <Button variant="outline" size="sm">
            <i className="fas fa-user-plus mr-2"></i>
            Add User
          </Button>
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          {mockUsers.map((user) => (
            <div key={user.id} className="flex items-center justify-between p-4 border border-slate-200 dark:border-slate-700 rounded-lg">
              <div className="flex items-center space-x-4">
                <div className="w-10 h-10 bg-brand-100 dark:bg-brand-900 rounded-full flex items-center justify-center">
                  <span className="text-brand-600 font-semibold">
                    {user.username.substring(0, 1).toUpperCase()}
                  </span>
                </div>
                <div>
                  <div className="flex items-center space-x-2">
                    <h4 className="font-medium text-slate-900 dark:text-white">
                      {user.username}
                    </h4>
                    <Badge variant={user.role === 'admin' ? 'destructive' : 'outline'}>
                      {user.role}
                    </Badge>
                    {user.isPremium && (
                      <Badge className="bg-amber-100 text-amber-800 dark:bg-amber-900 dark:text-amber-200">
                        <i className="fas fa-crown mr-1"></i>
                        Premium
                      </Badge>
                    )}
                  </div>
                  <p className="text-sm text-slate-600 dark:text-slate-400">
                    {user.email} • Joined {new Date(user.joinDate).toLocaleDateString()}
                  </p>
                </div>
              </div>
              <div className="flex items-center space-x-2">
                <Button variant="outline" size="sm">
                  <i className="fas fa-edit mr-2"></i>
                  Edit
                </Button>
                <Button variant="outline" size="sm" className="text-error-600 hover:text-error-700">
                  <i className="fas fa-ban mr-2"></i>
                  Ban
                </Button>
              </div>
            </div>
          ))}
        </div>

        <div className="mt-6 p-4 bg-slate-50 dark:bg-slate-800 rounded-lg">
          <h4 className="font-semibold text-slate-900 dark:text-white mb-3">
            Quick Actions
          </h4>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <Button variant="outline" className="justify-start">
              <i className="fas fa-download mr-2"></i>
              Export Users
            </Button>
            <Button variant="outline" className="justify-start">
              <i className="fas fa-envelope mr-2"></i>
              Send Announcement
            </Button>
            <Button variant="outline" className="justify-start">
              <i className="fas fa-chart-line mr-2"></i>
              User Analytics
            </Button>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

// Analytics Component
function Analytics() {
  const metrics = [
    { label: 'Daily Active Users', value: '2,847', change: '+12%', color: 'text-success-600' },
    { label: 'Problems Solved Today', value: '1,234', change: '+8%', color: 'text-brand-600' },
    { label: 'New Registrations', value: '89', change: '+15%', color: 'text-purple-600' },
    { label: 'Premium Conversions', value: '23', change: '+5%', color: 'text-amber-600' },
  ];

  const popularProblems = [
    { title: 'Two Sum', attempts: 1547, successRate: '89%' },
    { title: 'Valid Parentheses', attempts: 1203, successRate: '76%' },
    { title: 'Merge Two Sorted Lists', attempts: 967, successRate: '82%' },
    { title: 'Best Time to Buy and Sell Stock', attempts: 834, successRate: '71%' },
  ];

  return (
    <div className="space-y-6">
      {/* Metrics Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        {metrics.map((metric) => (
          <Card key={metric.label}>
            <CardContent className="p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-slate-600 dark:text-slate-400">{metric.label}</p>
                  <p className="text-2xl font-bold text-slate-900 dark:text-white">
                    {metric.value}
                  </p>
                </div>
                <div className={`text-sm font-medium ${metric.color}`}>
                  <i className="fas fa-arrow-up mr-1"></i>
                  {metric.change}
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Popular Problems */}
        <Card>
          <CardHeader>
            <CardTitle>
              <i className="fas fa-fire mr-2"></i>
              Popular Problems
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {popularProblems.map((problem, index) => (
                <div key={problem.title} className="flex items-center justify-between">
                  <div className="flex items-center space-x-3">
                    <div className="w-8 h-8 bg-brand-100 dark:bg-brand-900 rounded-full flex items-center justify-center">
                      <span className="text-brand-600 text-sm font-semibold">
                        {index + 1}
                      </span>
                    </div>
                    <div>
                      <h4 className="font-medium text-slate-900 dark:text-white">
                        {problem.title}
                      </h4>
                      <p className="text-sm text-slate-600 dark:text-slate-400">
                        {problem.attempts} attempts
                      </p>
                    </div>
                  </div>
                  <Badge variant="outline">{problem.successRate}</Badge>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        {/* System Status */}
        <Card>
          <CardHeader>
            <CardTitle>
              <i className="fas fa-heartbeat mr-2"></i>
              System Status
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {[
                { service: 'API Server', status: 'Operational', uptime: '99.9%' },
                { service: 'Database', status: 'Operational', uptime: '99.8%' },
                { service: 'Code Execution', status: 'Operational', uptime: '99.7%' },
                { service: 'AI Services', status: 'Operational', uptime: '99.6%' },
              ].map((service) => (
                <div key={service.service} className="flex items-center justify-between p-3 bg-success-50 dark:bg-success-900/20 rounded-lg">
                  <div className="flex items-center space-x-3">
                    <i className="fas fa-check-circle text-success-600"></i>
                    <div>
                      <h4 className="font-medium text-slate-900 dark:text-white">
                        {service.service}
                      </h4>
                      <p className="text-sm text-success-600">{service.status}</p>
                    </div>
                  </div>
                  <Badge variant="outline">{service.uptime}</Badge>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

export default function Admin() {
  return (
    <ProtectedRoute requireAdmin>
      <AdminContent />
    </ProtectedRoute>
  );
}
