
# Fix ServiceDetails.razor button
with open(r'd:\DEV\khadamat\src\Khadamat.BlazorUI\Pages\ServiceDetails.razor', 'r', encoding='utf-8') as f:
    c = f.read()

old = 'class="btn btn-info rounded-pill px-4 py-2 text-white fw-bold shadow-sm" @onclick="OpenRequestEditModal"'
new = 'class="btn btn-info rounded-pill px-3 py-1 text-white fw-bold shadow-sm flex-shrink-0" style="font-size:0.8rem; white-space:nowrap;" @onclick="OpenRequestEditModal"'

c2 = c.replace(old, new)
if c == c2:
    print('NOT FOUND - trying alternate')
    old2 = 'btn-info rounded-pill px-4 py-2'
    new2 = 'btn-info rounded-pill px-3 py-1'
    c2 = c.replace(old2, new2)
    if c == c2:
        print('STILL NOT FOUND')
    else:
        print('Found with alternate, replaced')
        with open(r'd:\DEV\khadamat\src\Khadamat.BlazorUI\Pages\ServiceDetails.razor', 'w', encoding='utf-8') as f:
            f.write(c2)
else:
    print('OK - replaced')
    with open(r'd:\DEV\khadamat\src\Khadamat.BlazorUI\Pages\ServiceDetails.razor', 'w', encoding='utf-8') as f:
        f.write(c2)
