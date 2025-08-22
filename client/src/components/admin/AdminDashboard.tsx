import { useQuery } from '@tanstack/react-query';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';

export function AdminDashboard() {
  const { data: stats, isLoading } = useQuery({
    queryKey: ['/api/admin/stats'],
  });

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
          {[...Array(4)].map((_, i) => (
            <Card key={i} className="animate-pulse">
              <CardContent className="p-6">
                <div className="flex items-center">
                  <div className="p-3 rounded-full bg-slate-200 dark:bg-slate-700">
                    <div className="w-6 h-6 bg-slate-300 dark:bg-slate-600 rounded"></div>
                  </div>
                  <div className="ml-4 flex-1">
                    <div className="h-8 bg-slate-200 dark:bg-slate-700 rounded mb-2"></div>
                    <div className="h-4 bg-slate-200 dark:bg-slate-700 rounded w-2/3"></div>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      </div>
    );
  }

  const statCards = [
    {
      title: 'Total Users',
      value: (stats as any)?.totalUsers?.toLocaleString() || '0',
      icon: 'fas fa-users',
      color: 'bg-blue-100 dark:bg-blue-900/20 text-blue-600',
    },
    {
      title: 'Problems',
      value: (stats as any)?.totalProblems?.toLocaleString() || '0',
      icon: 'fas fa-code',
      color: 'bg-success-100 dark:bg-success-900/20 text-success-600',
    },
    {
      title: 'Premium Users',
      value: (stats as any)?.premiumUsers?.toLocaleString() || '0',
      icon: 'fas fa-crown',
      color: 'bg-purple-100 dark:bg-purple-900/20 text-purple-600',
    },
    {
      title: 'Revenue',
      value: `$${(stats as any)?.revenue?.toLocaleString() || '0'}`,
      icon: 'fas fa-dollar-sign',
      color: 'bg-warning-100 dark:bg-warning-900/20 text-warning-600',
    },
  ];

  return (
    <div className="space-y-6">
      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        {statCards.map((stat) => (
          <Card key={stat.title}>
            <CardContent className="p-6">
              <div className="flex items-center">
                <div className={`p-3 rounded-full ${stat.color}`}>
                  <i className={`${stat.icon} text-lg`}></i>
                </div>
                <div className="ml-4">
                  <p className="text-2xl font-bold text-slate-900 dark:text-white">
                    {stat.value}
                  </p>
                  <p className="text-slate-600 dark:text-slate-400">{stat.title}</p>
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      {/* Recent Activity */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center">
            <i className="fas fa-chart-line mr-2"></i>
            Platform Overview
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <h4 className="font-semibold text-slate-900 dark:text-white mb-3">
                  Popular Categories
                </h4>
                <div className="space-y-2">
                  {[
                    { name: 'Arrays & Hashing', problems: 125, percentage: 85 },
                    { name: 'Trees', problems: 89, percentage: 70 },
                    { name: 'Dynamic Programming', problems: 67, percentage: 55 },
                    { name: 'Graphs', problems: 45, percentage: 40 },
                  ].map((category) => (
                    <div key={category.name} className="flex items-center justify-between">
                      <div>
                        <span className="text-sm font-medium text-slate-900 dark:text-white">
                          {category.name}
                        </span>
                        <span className="text-xs text-slate-500 ml-2">
                          {category.problems} problems
                        </span>
                      </div>
                      <div className="flex items-center space-x-2">
                        <div className="w-20 bg-slate-200 dark:bg-slate-700 rounded-full h-2">
                          <div
                            className="bg-brand-600 h-2 rounded-full"
                            style={{ width: `${category.percentage}%` }}
                          ></div>
                        </div>
                        <span className="text-xs text-slate-500 w-8">
                          {category.percentage}%
                        </span>
                      </div>
                    </div>
                  ))}
                </div>
              </div>

              <div>
                <h4 className="font-semibold text-slate-900 dark:text-white mb-3">
                  System Health
                </h4>
                <div className="space-y-3">
                  <div className="flex items-center justify-between p-3 bg-success-50 dark:bg-success-900/20 rounded-lg">
                    <div className="flex items-center">
                      <i className="fas fa-check-circle text-success-600 mr-2"></i>
                      <span className="text-sm font-medium">API Status</span>
                    </div>
                    <span className="text-sm text-success-600">Operational</span>
                  </div>
                  
                  <div className="flex items-center justify-between p-3 bg-success-50 dark:bg-success-900/20 rounded-lg">
                    <div className="flex items-center">
                      <i className="fas fa-database text-success-600 mr-2"></i>
                      <span className="text-sm font-medium">Database</span>
                    </div>
                    <span className="text-sm text-success-600">Healthy</span>
                  </div>
                  
                  <div className="flex items-center justify-between p-3 bg-success-50 dark:bg-success-900/20 rounded-lg">
                    <div className="flex items-center">
                      <i className="fas fa-robot text-success-600 mr-2"></i>
                      <span className="text-sm font-medium">AI Services</span>
                    </div>
                    <span className="text-sm text-success-600">Online</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
