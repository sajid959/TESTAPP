import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Button } from '@/components/ui/button';
import { ProblemTable } from '@/components/problems/ProblemTable';
import { ProblemFilters } from '@/components/problems/ProblemFilters';
import { Problem, Category } from '@shared/schema';
import { useAuth } from '@/hooks/useAuth';

export default function Problems() {
  const { user } = useAuth();
  const [filters, setFilters] = useState<{
    categoryId?: string;
    difficulty?: string;
    isPremium?: boolean;
    search?: string;
  }>({});

  const { data: categories = [] } = useQuery<Category[]>({
    queryKey: ['/api/categories'],
  });

  const { data: problems = [], isLoading } = useQuery<Problem[]>({
    queryKey: ['/api/problems', filters],
    queryFn: () => {
      const searchParams = new URLSearchParams();
      if (filters.categoryId) searchParams.set('categoryId', filters.categoryId);
      if (filters.difficulty) searchParams.set('difficulty', filters.difficulty);
      if (filters.isPremium !== undefined) searchParams.set('isPremium', String(filters.isPremium));
      if (filters.search) searchParams.set('search', filters.search);
      
      return fetch(`/api/problems?${searchParams.toString()}`, {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('accessToken')}`,
        },
      }).then(res => res.json());
    },
  });

  const handleFiltersChange = (newFilters: typeof filters) => {
    setFilters(newFilters);
  };

  return (
    <div className="min-h-screen bg-slate-50 dark:bg-slate-900">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Header */}
        <div className="flex flex-col lg:flex-row justify-between items-start lg:items-center mb-8 gap-4">
          <div>
            <h1 className="text-3xl font-bold text-slate-900 dark:text-white mb-2">
              Practice Problems
            </h1>
            <p className="text-slate-600 dark:text-slate-400">
              {problems.length > 0 && (
                <>Showing {problems.length} problem{problems.length !== 1 ? 's' : ''}</>
              )}
              {Object.keys(filters).length > 0 && ' with filters applied'}
            </p>
          </div>
          
          {user?.role === 'admin' && (
            <Button className="bg-success-600 hover:bg-success-700" data-testid="button-admin-add-problem">
              <i className="fas fa-plus mr-2"></i>
              New Problem
            </Button>
          )}
        </div>

        {/* Filters */}
        <ProblemFilters
          categories={categories}
          filters={filters}
          onFiltersChange={handleFiltersChange}
        />

        {/* Quick Stats */}
        {!isLoading && problems.length > 0 && (
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
            <div className="bg-white dark:bg-slate-800 rounded-lg p-4 border border-slate-200 dark:border-slate-700">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-slate-600 dark:text-slate-400">Total Problems</p>
                  <p className="text-2xl font-bold text-slate-900 dark:text-white">{problems.length}</p>
                </div>
                <i className="fas fa-code text-2xl text-brand-600"></i>
              </div>
            </div>
            
            <div className="bg-white dark:bg-slate-800 rounded-lg p-4 border border-slate-200 dark:border-slate-700">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-slate-600 dark:text-slate-400">Easy</p>
                  <p className="text-2xl font-bold text-success-600">
                    {problems.filter(p => p.difficulty === 'Easy').length}
                  </p>
                </div>
                <i className="fas fa-smile text-2xl text-success-600"></i>
              </div>
            </div>
            
            <div className="bg-white dark:bg-slate-800 rounded-lg p-4 border border-slate-200 dark:border-slate-700">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-slate-600 dark:text-slate-400">Medium</p>
                  <p className="text-2xl font-bold text-warning-600">
                    {problems.filter(p => p.difficulty === 'Medium').length}
                  </p>
                </div>
                <i className="fas fa-meh text-2xl text-warning-600"></i>
              </div>
            </div>
            
            <div className="bg-white dark:bg-slate-800 rounded-lg p-4 border border-slate-200 dark:border-slate-700">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-slate-600 dark:text-slate-400">Hard</p>
                  <p className="text-2xl font-bold text-error-600">
                    {problems.filter(p => p.difficulty === 'Hard').length}
                  </p>
                </div>
                <i className="fas fa-fire text-2xl text-error-600"></i>
              </div>
            </div>
          </div>
        )}

        {/* Problems Table */}
        <ProblemTable problems={problems} isLoading={isLoading} />

        {/* Premium Upgrade CTA */}
        {!user?.isPremium && problems.some(p => p.isPremium) && (
          <div className="mt-8 bg-gradient-to-r from-brand-500 to-brand-600 rounded-xl p-6 text-white">
            <div className="flex flex-col md:flex-row items-center justify-between">
              <div>
                <h3 className="text-xl font-semibold mb-2">
                  <i className="fas fa-crown mr-2 text-amber-300"></i>
                  Unlock Premium Problems
                </h3>
                <p className="text-brand-100">
                  Get access to {problems.filter(p => p.isPremium).length} premium problems with detailed solutions and exclusive content.
                </p>
              </div>
              <Button 
                className="mt-4 md:mt-0 bg-white text-brand-600 hover:bg-brand-50"
                data-testid="button-upgrade-premium"
              >
                <i className="fas fa-arrow-up mr-2"></i>
                Upgrade Now
              </Button>
            </div>
          </div>
        )}

        {/* Help Section */}
        {problems.length === 0 && !isLoading && (
          <div className="text-center py-12">
            <div className="max-w-md mx-auto">
              <i className="fas fa-search text-6xl text-slate-300 dark:text-slate-600 mb-6"></i>
              <h3 className="text-xl font-semibold text-slate-900 dark:text-white mb-4">
                No problems found
              </h3>
              <p className="text-slate-600 dark:text-slate-400 mb-6">
                Try adjusting your search filters or explore different categories.
              </p>
              <Button
                onClick={() => setFilters({})}
                variant="outline"
                data-testid="button-reset-filters"
              >
                <i className="fas fa-refresh mr-2"></i>
                Clear All Filters
              </Button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
