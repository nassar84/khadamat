# Admin Control Testing Plan & Checklist

This document outlines the testing procedures to verify the Administrator's control over the Khadamat system.

## 1. User Management (System Admin Only)
- [ ] **View Users List**: verify that the `/admin/users` page loads all users.
- [ ] **Filter Users**:
    - [ ] Search by Name/Email.
    - [ ] Filter by Role (User, System Admin, Super Admin).
- [ ] **User Actions**:
    - [ ] **Edit User**: Update a user's name, phone, or bio. verify persistence.
    - [ ] **Change Role**: Promote a "User" to "System Admin" and verify they can access the admin panel.
    - [ ] **Toggle Status**: Deactivate a user and verify they cannot login.
    - [ ] **Delete User**: Delete a test user and verify they are removed from the list.

## 2. Service Management
- [ ] **View Services Tree**: Verify the tree view in `/admin/services` expands and collapses correctly.
- [ ] **Service Approval**:
    - [ ] Go to `/admin/approvals/services`.
    - [ ] Approve a pending service. Verify it appears in the public "Services" page.
    - [ ] Reject a pending service. Verify it is marked as rejected or removed.
- [ ] **Edit Service**:
    - [ ] Open a service details page as Admin.
    - [ ] Click "Edit". Verify you can change the Title, Description, and Price.
    - [ ] Save and refresh to confirm changes.
- [ ] **Delete Service**: Delete a service from the admin tree view.

## 3. Provider Management
- [ ] **Pending Providers**:
    - [ ] Go to `/admin/approvals/providers`.
    - [ ] View attached documents (ID, Certificate) - ensure images load.
    - [ ] **Approve**: Approve a provider. Verify their profile becomes "Verified".
    - [ ] **Reject**: Reject a provider.

## 4. Categories & Locations
- [ ] **Categories**:
    - [ ] Add a new Main Category.
    - [ ] Add a Sub-Category.
    - [ ] Edit/Delete existing categories.
- [ ] **Locations**:
    - [ ] Add a new Governorate.
    - [ ] Add a City to that Governorate.

## 5. System Settings (Super Admin Only)
- [ ] **Ads**: Create a new ad banner and verify it appears on the Home page.
- [ ] **Reports**: Check if reports load data (even if dummy data).

## 6. Access Control Tests
- [ ] **Login as System Admin**: Verify you CANNOT see "Super Admin" actions (if configured).
- [ ] **Login as User**: Verify you CANNOT access `/admin` URLs (should redirect to login or home).
