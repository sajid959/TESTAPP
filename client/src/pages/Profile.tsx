import { useState } from 'react';
import * as React from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Badge } from '@/components/ui/badge';
import { Separator } from '@/components/ui/separator';
import { useAuth } from '@/hooks/useAuth';
import { useToast } from '@/hooks/use-toast';
import { apiRequest } from '@/lib/queryClient';
import { User, Crown, Mail, Calendar, MapPin, Building, Globe } from 'lucide-react';

interface UserProfile {
  bio?: string;
  location?: string;
  website?: string;
  company?: string;
  skills: string[];
}

export default function Profile() {
  const { user } = useAuth();
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const [isEditing, setIsEditing] = useState(false);
  const [profileData, setProfileData] = useState<UserProfile>({
    bio: '',
    location: '',
    website: '',
    company: '',
    skills: []
  });

  // Fetch user profile data
  const { data: profile, isLoading } = useQuery({
    queryKey: ['/api/auth/me'],
    queryFn: async () => {
      const response = await apiRequest('GET', '/api/auth/me');
      return response.json();
    }
  });

  // Update profile data when query data changes
  React.useEffect(() => {
    if (profile?.user?.profile) {
      setProfileData(profile.user.profile);
    }
  }, [profile]);

  // Update profile mutation
  const updateProfileMutation = useMutation({
    mutationFn: async (data: UserProfile) => {
      const response = await apiRequest('PUT', '/api/auth/profile', data);
      return response.json();
    },
    onSuccess: () => {
      toast({
        title: 'Profile Updated',
        description: 'Your profile has been successfully updated.',
      });
      queryClient.invalidateQueries({ queryKey: ['/api/auth/me'] });
      setIsEditing(false);
    },
    onError: (error: any) => {
      toast({
        title: 'Error',
        description: error.message || 'Failed to update profile.',
        variant: 'destructive',
      });
    }
  });

  const handleSave = () => {
    updateProfileMutation.mutate(profileData);
  };

  const handleCancel = () => {
    if (profile?.user?.profile) {
      setProfileData(profile.user.profile);
    }
    setIsEditing(false);
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
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold text-slate-900 dark:text-white">Profile</h1>
            <p className="text-slate-600 dark:text-slate-400">
              Manage your account information and preferences
            </p>
          </div>
          <div className="flex space-x-2">
            {!isEditing ? (
              <Button onClick={() => setIsEditing(true)} data-testid="button-edit-profile">
                Edit Profile
              </Button>
            ) : (
              <>
                <Button 
                  variant="outline" 
                  onClick={handleCancel}
                  data-testid="button-cancel-edit"
                >
                  Cancel
                </Button>
                <Button 
                  onClick={handleSave}
                  disabled={updateProfileMutation.isPending}
                  data-testid="button-save-profile"
                >
                  {updateProfileMutation.isPending ? 'Saving...' : 'Save Changes'}
                </Button>
              </>
            )}
          </div>
        </div>

        <div className="grid gap-6 md:grid-cols-3">
          {/* Basic Info Card */}
          <Card className="md:col-span-1">
            <CardHeader>
              <CardTitle className="flex items-center space-x-2">
                <User className="w-5 h-5" />
                <span>Basic Information</span>
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex items-center space-x-3">
                <div className="w-16 h-16 bg-gradient-to-r from-blue-500 to-purple-600 rounded-full flex items-center justify-center text-white font-bold text-xl">
                  {((user as any)?.username || user?.email?.split('@')[0] || 'U').slice(0, 2).toUpperCase()}
                </div>
                <div>
                  <h3 className="text-lg font-semibold text-slate-900 dark:text-white">
                    {(user as any)?.username || user?.email?.split('@')[0]}
                  </h3>
                  <div className="flex items-center space-x-2">
                    {user?.subscriptionStatus === 'premium' && (
                      <Crown className="w-4 h-4 text-yellow-500" />
                    )}
                    <Badge variant={user?.subscriptionStatus === 'premium' ? 'default' : 'secondary'}>
                      {user?.subscriptionStatus === 'premium' ? 'Premium' : 'Free'}
                    </Badge>
                  </div>
                </div>
              </div>
              
              <Separator />
              
              <div className="space-y-3 text-sm">
                <div className="flex items-center space-x-2 text-slate-600 dark:text-slate-400">
                  <Mail className="w-4 h-4" />
                  <span>{user?.email}</span>
                </div>
                <div className="flex items-center space-x-2 text-slate-600 dark:text-slate-400">
                  <Calendar className="w-4 h-4" />
                  <span>Joined {new Date(user?.createdAt || '').toLocaleDateString()}</span>
                </div>
              </div>

              <Separator />

              <div className="space-y-2">
                <h4 className="font-medium text-slate-900 dark:text-white">Statistics</h4>
                <div className="grid grid-cols-2 gap-4 text-center">
                  <div>
                    <div className="text-2xl font-bold text-blue-600 dark:text-blue-400">
                      {user?.totalSolved || 0}
                    </div>
                    <div className="text-xs text-slate-600 dark:text-slate-400">Solved</div>
                  </div>
                  <div>
                    <div className="text-2xl font-bold text-green-600 dark:text-green-400">
                      {(user as any)?.acceptanceRate || 0}%
                    </div>
                    <div className="text-xs text-slate-600 dark:text-slate-400">Acceptance</div>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Profile Details Card */}
          <Card className="md:col-span-2">
            <CardHeader>
              <CardTitle>Profile Details</CardTitle>
              <CardDescription>
                Tell others about yourself and your coding journey
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="grid gap-4 md:grid-cols-2">
                <div className="space-y-2">
                  <Label htmlFor="location">Location</Label>
                  {isEditing ? (
                    <div className="relative">
                      <MapPin className="absolute left-3 top-3 w-4 h-4 text-slate-400" />
                      <Input
                        id="location"
                        placeholder="e.g., San Francisco, CA"
                        value={profileData.location || ''}
                        onChange={(e) => setProfileData({ ...profileData, location: e.target.value })}
                        className="pl-10"
                        data-testid="input-location"
                      />
                    </div>
                  ) : (
                    <div className="flex items-center space-x-2 text-slate-600 dark:text-slate-400">
                      <MapPin className="w-4 h-4" />
                      <span>{profileData.location || 'Not specified'}</span>
                    </div>
                  )}
                </div>

                <div className="space-y-2">
                  <Label htmlFor="company">Company</Label>
                  {isEditing ? (
                    <div className="relative">
                      <Building className="absolute left-3 top-3 w-4 h-4 text-slate-400" />
                      <Input
                        id="company"
                        placeholder="e.g., Google, Microsoft"
                        value={profileData.company || ''}
                        onChange={(e) => setProfileData({ ...profileData, company: e.target.value })}
                        className="pl-10"
                        data-testid="input-company"
                      />
                    </div>
                  ) : (
                    <div className="flex items-center space-x-2 text-slate-600 dark:text-slate-400">
                      <Building className="w-4 h-4" />
                      <span>{profileData.company || 'Not specified'}</span>
                    </div>
                  )}
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="website">Website</Label>
                {isEditing ? (
                  <div className="relative">
                    <Globe className="absolute left-3 top-3 w-4 h-4 text-slate-400" />
                    <Input
                      id="website"
                      placeholder="https://yourwebsite.com"
                      value={profileData.website || ''}
                      onChange={(e) => setProfileData({ ...profileData, website: e.target.value })}
                      className="pl-10"
                      data-testid="input-website"
                    />
                  </div>
                ) : (
                  <div className="flex items-center space-x-2 text-slate-600 dark:text-slate-400">
                    <Globe className="w-4 h-4" />
                    {profileData.website ? (
                      <a 
                        href={profileData.website} 
                        target="_blank" 
                        rel="noopener noreferrer"
                        className="text-blue-600 dark:text-blue-400 hover:underline"
                      >
                        {profileData.website}
                      </a>
                    ) : (
                      <span>Not specified</span>
                    )}
                  </div>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="bio">Bio</Label>
                {isEditing ? (
                  <Textarea
                    id="bio"
                    placeholder="Tell us about yourself, your experience, and what you're passionate about..."
                    value={profileData.bio || ''}
                    onChange={(e) => setProfileData({ ...profileData, bio: e.target.value })}
                    rows={4}
                    data-testid="textarea-bio"
                  />
                ) : (
                  <div className="min-h-[100px] p-3 bg-slate-50 dark:bg-slate-800 rounded-md">
                    <p className="text-slate-700 dark:text-slate-300 whitespace-pre-wrap">
                      {profileData.bio || 'No bio provided yet.'}
                    </p>
                  </div>
                )}
              </div>

              <div className="space-y-2">
                <Label>Skills & Technologies</Label>
                <div className="flex flex-wrap gap-2">
                  {profileData.skills && profileData.skills.length > 0 ? (
                    profileData.skills.map((skill, index) => (
                      <Badge key={index} variant="secondary" data-testid={`badge-skill-${index}`}>
                        {skill}
                      </Badge>
                    ))
                  ) : (
                    <p className="text-slate-600 dark:text-slate-400 text-sm">No skills added yet.</p>
                  )}
                </div>
                {isEditing && (
                  <p className="text-xs text-slate-600 dark:text-slate-400">
                    Skills management will be available in the settings page.
                  </p>
                )}
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}