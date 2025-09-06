import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Switch } from '@/components/ui/switch';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Separator } from '@/components/ui/separator';
import { Badge } from '@/components/ui/badge';
import { useAuth } from '@/hooks/useAuth';
import { useToast } from '@/hooks/use-toast';
import { apiRequest } from '@/lib/queryClient';
import { Shield, Bell, Globe, Palette, Key, Trash2, Plus, X } from 'lucide-react';

interface UserPreferences {
  theme: string;
  language: string;
  notifications: {
    email: boolean;
    push: boolean;
    contests: boolean;
    submissions: boolean;
  };
}

interface UserProfile {
  skills: string[];
}

export default function SettingsPage() {
  const { logout } = useAuth();
  const { toast } = useToast();
  const queryClient = useQueryClient();
  
  const [preferences, setPreferences] = useState<UserPreferences>({
    theme: 'system',
    language: 'en',
    notifications: {
      email: true,
      push: true,
      contests: true,
      submissions: true
    }
  });

  const [profile, setProfile] = useState<UserProfile>({
    skills: []
  });

  const [newSkill, setNewSkill] = useState('');
  const [passwordData, setPasswordData] = useState({
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  });

  // Fetch user settings
  const { isLoading } = useQuery({
    queryKey: ['/api/auth/me'],
    queryFn: async () => {
      const response = await apiRequest('GET', '/api/auth/me');
      const data = await response.json();
      if (data.user?.preferences) {
        setPreferences(data.user.preferences);
      }
      if (data.user?.profile) {
        setProfile(data.user.profile);
      }
      return data;
    }
  });

  // Update preferences mutation
  const updatePreferencesMutation = useMutation({
    mutationFn: async (data: UserPreferences) => {
      const response = await apiRequest('PUT', '/api/auth/preferences', data);
      return response.json();
    },
    onSuccess: () => {
      toast({
        title: 'Settings Updated',
        description: 'Your preferences have been successfully updated.',
      });
      queryClient.invalidateQueries({ queryKey: ['/api/auth/me'] });
    },
    onError: (error: any) => {
      toast({
        title: 'Error',
        description: error.message || 'Failed to update settings.',
        variant: 'destructive',
      });
    }
  });

  // Update profile mutation
  const updateProfileMutation = useMutation({
    mutationFn: async (data: UserProfile) => {
      const response = await apiRequest('PUT', '/api/auth/profile', data);
      return response.json();
    },
    onSuccess: () => {
      toast({
        title: 'Profile Updated',
        description: 'Your skills have been successfully updated.',
      });
      queryClient.invalidateQueries({ queryKey: ['/api/auth/me'] });
    },
    onError: (error: any) => {
      toast({
        title: 'Error',
        description: error.message || 'Failed to update profile.',
        variant: 'destructive',
      });
    }
  });

  // Change password mutation
  const changePasswordMutation = useMutation({
    mutationFn: async (data: typeof passwordData) => {
      const response = await apiRequest('POST', '/api/auth/change-password', {
        currentPassword: data.currentPassword,
        newPassword: data.newPassword
      });
      return response.json();
    },
    onSuccess: () => {
      toast({
        title: 'Password Changed',
        description: 'Your password has been successfully updated.',
      });
      setPasswordData({ currentPassword: '', newPassword: '', confirmPassword: '' });
    },
    onError: (error: any) => {
      toast({
        title: 'Error',
        description: error.message || 'Failed to change password.',
        variant: 'destructive',
      });
    }
  });

  const handleAddSkill = () => {
    if (newSkill.trim() && !profile.skills.includes(newSkill.trim())) {
      const updatedProfile = {
        ...profile,
        skills: [...profile.skills, newSkill.trim()]
      };
      setProfile(updatedProfile);
      updateProfileMutation.mutate(updatedProfile);
      setNewSkill('');
    }
  };

  const handleRemoveSkill = (skillToRemove: string) => {
    const updatedProfile = {
      ...profile,
      skills: profile.skills.filter(skill => skill !== skillToRemove)
    };
    setProfile(updatedProfile);
    updateProfileMutation.mutate(updatedProfile);
  };

  const handleChangePassword = () => {
    if (passwordData.newPassword !== passwordData.confirmPassword) {
      toast({
        title: 'Error',
        description: 'New passwords do not match.',
        variant: 'destructive',
      });
      return;
    }
    if (passwordData.newPassword.length < 8) {
      toast({
        title: 'Error',
        description: 'Password must be at least 8 characters long.',
        variant: 'destructive',
      });
      return;
    }
    changePasswordMutation.mutate(passwordData);
  };

  if (isLoading) {
    return (
      <div className="container mx-auto py-8">
        <div className="flex items-center justify-center min-h-[400px]">
          <div className="animate-spin w-8 h-8 border-4 border-primary border-t-transparent rounded-full" />
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto py-8 max-w-4xl">
      <div className="space-y-6">
        {/* Header */}
        <div>
          <h1 className="text-3xl font-bold text-slate-900 dark:text-white">Settings</h1>
          <p className="text-slate-600 dark:text-slate-400">
            Manage your account settings and preferences
          </p>
        </div>

        {/* Appearance Settings */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center space-x-2">
              <Palette className="w-5 h-5" />
              <span>Appearance</span>
            </CardTitle>
            <CardDescription>
              Customize how DSAGrind looks and feels
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="theme">Theme</Label>
                <Select 
                  value={preferences.theme} 
                  onValueChange={(value) => {
                    const updated = { ...preferences, theme: value };
                    setPreferences(updated);
                    updatePreferencesMutation.mutate(updated);
                  }}
                >
                  <SelectTrigger data-testid="select-theme">
                    <SelectValue placeholder="Select theme" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="light">Light</SelectItem>
                    <SelectItem value="dark">Dark</SelectItem>
                    <SelectItem value="system">System</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label htmlFor="language">Language</Label>
                <Select 
                  value={preferences.language} 
                  onValueChange={(value) => {
                    const updated = { ...preferences, language: value };
                    setPreferences(updated);
                    updatePreferencesMutation.mutate(updated);
                  }}
                >
                  <SelectTrigger data-testid="select-language">
                    <SelectValue placeholder="Select language" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="en">English</SelectItem>
                    <SelectItem value="es">Spanish</SelectItem>
                    <SelectItem value="fr">French</SelectItem>
                    <SelectItem value="de">German</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Notification Settings */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center space-x-2">
              <Bell className="w-5 h-5" />
              <span>Notifications</span>
            </CardTitle>
            <CardDescription>
              Configure when and how you receive notifications
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <div>
                  <Label htmlFor="email-notifications">Email Notifications</Label>
                  <p className="text-sm text-slate-600 dark:text-slate-400">
                    Receive notifications via email
                  </p>
                </div>
                <Switch
                  id="email-notifications"
                  checked={preferences.notifications.email}
                  onCheckedChange={(checked) => {
                    const updated = {
                      ...preferences,
                      notifications: { ...preferences.notifications, email: checked }
                    };
                    setPreferences(updated);
                    updatePreferencesMutation.mutate(updated);
                  }}
                  data-testid="switch-email-notifications"
                />
              </div>

              <div className="flex items-center justify-between">
                <div>
                  <Label htmlFor="push-notifications">Push Notifications</Label>
                  <p className="text-sm text-slate-600 dark:text-slate-400">
                    Receive push notifications in your browser
                  </p>
                </div>
                <Switch
                  id="push-notifications"
                  checked={preferences.notifications.push}
                  onCheckedChange={(checked) => {
                    const updated = {
                      ...preferences,
                      notifications: { ...preferences.notifications, push: checked }
                    };
                    setPreferences(updated);
                    updatePreferencesMutation.mutate(updated);
                  }}
                  data-testid="switch-push-notifications"
                />
              </div>

              <div className="flex items-center justify-between">
                <div>
                  <Label htmlFor="contest-notifications">Contest Reminders</Label>
                  <p className="text-sm text-slate-600 dark:text-slate-400">
                    Get notified about upcoming contests
                  </p>
                </div>
                <Switch
                  id="contest-notifications"
                  checked={preferences.notifications.contests}
                  onCheckedChange={(checked) => {
                    const updated = {
                      ...preferences,
                      notifications: { ...preferences.notifications, contests: checked }
                    };
                    setPreferences(updated);
                    updatePreferencesMutation.mutate(updated);
                  }}
                  data-testid="switch-contest-notifications"
                />
              </div>

              <div className="flex items-center justify-between">
                <div>
                  <Label htmlFor="submission-notifications">Submission Results</Label>
                  <p className="text-sm text-slate-600 dark:text-slate-400">
                    Get notified when your submissions are processed
                  </p>
                </div>
                <Switch
                  id="submission-notifications"
                  checked={preferences.notifications.submissions}
                  onCheckedChange={(checked) => {
                    const updated = {
                      ...preferences,
                      notifications: { ...preferences.notifications, submissions: checked }
                    };
                    setPreferences(updated);
                    updatePreferencesMutation.mutate(updated);
                  }}
                  data-testid="switch-submission-notifications"
                />
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Skills Management */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center space-x-2">
              <Globe className="w-5 h-5" />
              <span>Skills & Technologies</span>
            </CardTitle>
            <CardDescription>
              Manage your programming skills and technologies
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex space-x-2">
              <Input
                placeholder="Add a skill (e.g., Python, React, AWS)"
                value={newSkill}
                onChange={(e) => setNewSkill(e.target.value)}
                onKeyPress={(e) => e.key === 'Enter' && handleAddSkill()}
                data-testid="input-new-skill"
              />
              <Button onClick={handleAddSkill} size="sm" data-testid="button-add-skill">
                <Plus className="w-4 h-4" />
              </Button>
            </div>
            <div className="flex flex-wrap gap-2">
              {profile.skills.map((skill, index) => (
                <Badge key={index} variant="secondary" className="flex items-center space-x-1">
                  <span>{skill}</span>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => handleRemoveSkill(skill)}
                    className="h-auto p-0 w-4 h-4 hover:bg-transparent"
                    data-testid={`button-remove-skill-${index}`}
                  >
                    <X className="w-3 h-3" />
                  </Button>
                </Badge>
              ))}
            </div>
            {profile.skills.length === 0 && (
              <p className="text-slate-600 dark:text-slate-400 text-sm">
                No skills added yet. Add your first skill above!
              </p>
            )}
          </CardContent>
        </Card>

        {/* Security Settings */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center space-x-2">
              <Shield className="w-5 h-5" />
              <span>Security</span>
            </CardTitle>
            <CardDescription>
              Manage your account security and password
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid gap-4 md:grid-cols-1 max-w-md">
              <div className="space-y-2">
                <Label htmlFor="current-password">Current Password</Label>
                <Input
                  id="current-password"
                  type="password"
                  value={passwordData.currentPassword}
                  onChange={(e) => setPasswordData({ ...passwordData, currentPassword: e.target.value })}
                  data-testid="input-current-password"
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="new-password">New Password</Label>
                <Input
                  id="new-password"
                  type="password"
                  value={passwordData.newPassword}
                  onChange={(e) => setPasswordData({ ...passwordData, newPassword: e.target.value })}
                  data-testid="input-new-password"
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="confirm-password">Confirm New Password</Label>
                <Input
                  id="confirm-password"
                  type="password"
                  value={passwordData.confirmPassword}
                  onChange={(e) => setPasswordData({ ...passwordData, confirmPassword: e.target.value })}
                  data-testid="input-confirm-password"
                />
              </div>

              <Button 
                onClick={handleChangePassword}
                disabled={!passwordData.currentPassword || !passwordData.newPassword || !passwordData.confirmPassword || changePasswordMutation.isPending}
                className="w-full"
                data-testid="button-change-password"
              >
                <Key className="w-4 h-4 mr-2" />
                {changePasswordMutation.isPending ? 'Changing Password...' : 'Change Password'}
              </Button>
            </div>
          </CardContent>
        </Card>

        {/* Account Management */}
        <Card className="border-red-200 dark:border-red-800">
          <CardHeader>
            <CardTitle className="flex items-center space-x-2 text-red-600 dark:text-red-400">
              <Trash2 className="w-5 h-5" />
              <span>Danger Zone</span>
            </CardTitle>
            <CardDescription>
              Irreversible and destructive actions
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <h4 className="font-medium text-slate-900 dark:text-white">Sign Out</h4>
              <p className="text-sm text-slate-600 dark:text-slate-400">
                Sign out of your account on this device
              </p>
              <Button 
                variant="outline" 
                onClick={logout}
                data-testid="button-sign-out"
              >
                Sign Out
              </Button>
            </div>

            <Separator />

            <div className="space-y-2">
              <h4 className="font-medium text-red-600 dark:text-red-400">Delete Account</h4>
              <p className="text-sm text-slate-600 dark:text-slate-400">
                Permanently delete your account and all associated data. This action cannot be undone.
              </p>
              <Button 
                variant="destructive" 
                onClick={() => {
                  toast({
                    title: 'Feature Not Available',
                    description: 'Account deletion is not yet implemented. Please contact support.',
                    variant: 'destructive',
                  });
                }}
                data-testid="button-delete-account"
              >
                <Trash2 className="w-4 h-4 mr-2" />
                Delete Account
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}