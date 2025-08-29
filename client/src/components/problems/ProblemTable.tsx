import { useState } from 'react';
import { Link } from 'wouter';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Tooltip, TooltipContent, TooltipTrigger } from '@/components/ui/tooltip';
import { Problem } from '@/types/api';
import { useAuth } from '@/hooks/useAuth';

interface ProblemTableProps {
  problems: Problem[];
  isLoading?: boolean;
}

export function ProblemTable({ problems, isLoading = false }: ProblemTableProps) {
  const { user } = useAuth();
  const [hoveredRow, setHoveredRow] = useState<string | null>(null);

  const getDifficultyBadgeClass = (difficulty: string) => {
    switch (difficulty.toLowerCase()) {
      case 'easy':
        return 'difficulty-easy';
      case 'medium':
        return 'difficulty-medium';
      case 'hard':
        return 'difficulty-hard';
      default:
        return '';
    }
  };

  const getStatusIcon = (problemId: string) => {
    // This would normally come from user progress data
    // For now, we'll show a mock status
    const statuses = ['solved', 'attempted', 'not-started'];
    const status = statuses[Math.floor(Math.random() * statuses.length)];
    
    switch (status) {
      case 'solved':
        return <i className="fas fa-check-circle text-success-500 text-lg" />;
      case 'attempted':
        return <i className="fas fa-minus-circle text-warning-500 text-lg" />;
      default:
        return <i className="fas fa-circle text-slate-300 dark:text-slate-600 text-lg" />;
    }
  };

  const canAccessProblem = (problem: Problem) => {
    return !problem.isPaid || (user?.subscriptionPlan === 'premium' && user?.subscriptionStatus === 'active');
  };

  if (isLoading) {
    return (
      <div className="bg-white dark:bg-slate-900 rounded-xl shadow-sm border border-slate-200 dark:border-slate-800 overflow-hidden">
        <div className="animate-pulse">
          {[...Array(5)].map((_, i) => (
            <div key={i} className="px-6 py-4 border-b border-slate-200 dark:border-slate-700">
              <div className="flex items-center space-x-4">
                <div className="w-6 h-6 bg-slate-200 dark:bg-slate-700 rounded-full"></div>
                <div className="flex-1">
                  <div className="h-4 bg-slate-200 dark:bg-slate-700 rounded w-1/3 mb-2"></div>
                  <div className="h-3 bg-slate-200 dark:bg-slate-700 rounded w-1/4"></div>
                </div>
                <div className="w-16 h-6 bg-slate-200 dark:bg-slate-700 rounded"></div>
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  if (problems.length === 0) {
    return (
      <div className="bg-white dark:bg-slate-900 rounded-xl shadow-sm border border-slate-200 dark:border-slate-800 p-12 text-center">
        <i className="fas fa-search text-4xl text-slate-300 dark:text-slate-600 mb-4"></i>
        <h3 className="text-lg font-semibold text-slate-900 dark:text-white mb-2">
          No problems found
        </h3>
        <p className="text-slate-600 dark:text-slate-400">
          Try adjusting your filters or search terms.
        </p>
      </div>
    );
  }

  return (
    <div className="bg-white dark:bg-slate-900 rounded-xl shadow-sm border border-slate-200 dark:border-slate-800 overflow-hidden">
      <Table>
        <TableHeader>
          <TableRow className="bg-slate-50 dark:bg-slate-800 border-b border-slate-200 dark:border-slate-700">
            <TableHead className="w-16">Status</TableHead>
            <TableHead>Title</TableHead>
            <TableHead className="w-24">Acceptance</TableHead>
            <TableHead className="w-32">Difficulty</TableHead>
            <TableHead>Tags</TableHead>
            <TableHead className="w-20">Access</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {problems.map((problem, index) => (
            <TableRow
              key={problem.id}
              className={`hover:bg-slate-50 dark:hover:bg-slate-800 cursor-pointer transition-colors ${
                hoveredRow === problem.id ? 'bg-slate-50 dark:bg-slate-800' : ''
              }`}
              onMouseEnter={() => setHoveredRow(problem.id)}
              onMouseLeave={() => setHoveredRow(null)}
              data-testid={`row-problem-${index}`}
            >
              <TableCell>
                <Tooltip>
                  <TooltipTrigger>
                    {getStatusIcon(problem.id)}
                  </TooltipTrigger>
                  <TooltipContent>
                    <p>Problem status</p>
                  </TooltipContent>
                </Tooltip>
              </TableCell>
              
              <TableCell>
                {canAccessProblem(problem) ? (
                  <Link href={`/problem/${problem.id}`}>
                    <Button 
                      variant="link" 
                      className="p-0 h-auto font-medium text-slate-900 dark:text-white hover:text-brand-600 dark:hover:text-brand-400"
                      data-testid={`link-problem-${problem.title.replace(/\s+/g, '-').toLowerCase()}`}
                    >
                      {index + 1}. {problem.title}
                    </Button>
                  </Link>
                ) : (
                  <div className="flex items-center space-x-2">
                    <span className="font-medium text-slate-900 dark:text-white">
                      {index + 1}. {problem.title}
                    </span>
                    <i className="fas fa-lock text-amber-500 text-sm" />
                  </div>
                )}
              </TableCell>
              
              <TableCell>
                <span className="text-slate-600 dark:text-slate-400">
                  {problem.acceptanceRate}%
                </span>
              </TableCell>
              
              <TableCell>
                <Badge className={getDifficultyBadgeClass(problem.difficulty)}>
                  {problem.difficulty}
                </Badge>
              </TableCell>
              
              <TableCell>
                <div className="flex flex-wrap gap-1">
                  {(Array.isArray(problem.tags) ? problem.tags : []).slice(0, 3).map((tag: string) => (
                    <Badge
                      key={tag}
                      variant="outline"
                      className="text-xs bg-blue-50 text-blue-800 border-blue-200 dark:bg-blue-900/20 dark:text-blue-300 dark:border-blue-800"
                    >
                      {tag}
                    </Badge>
                  ))}
                  {Array.isArray(problem.tags) && problem.tags.length > 3 && (
                    <Badge variant="outline" className="text-xs">
                      +{problem.tags.length - 3}
                    </Badge>
                  )}
                </div>
              </TableCell>
              
              <TableCell>
                {problem.isPremium ? (
                  <Tooltip>
                    <TooltipTrigger>
                      <i className="fas fa-crown text-amber-500" />
                    </TooltipTrigger>
                    <TooltipContent>
                      <p>Premium only</p>
                    </TooltipContent>
                  </Tooltip>
                ) : (
                  <span className="text-slate-400 dark:text-slate-600 text-sm">Free</span>
                )}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
