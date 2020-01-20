
# Net Core 3.1 Cookie Register / Login using email code verification
# Bootstrap & JQuery 
1. Entity Framework Core Tools
2. Microsoft.EntityFrameworkCore.SqlServer
3. Microsoft.AspNetCore.Mvc.TagHelpers
4. SendGrid

Create Models: 
 Scaffold-DbContext "Server=.;Database=youtube; User Id = youtube; Password = password; Trusted_Connection=True;MultipleActiveResultSets=true" 
 Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -force
 
 # DataBase 
 <pre>CREATE TABLE [dbo].[users](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [username] [nvarchar](20) NOT NULL,
    [primary_name] [nvarchar](20) NOT NULL,
    [second_name] [nvarchar](20) NOT NULL,
    [email] [nvarchar](254) NOT NULL,
    [password] [nvarchar](60) NOT NULL,
    [salt] [varchar](60) NOT NULL,
    [joined_date] [datetime2](7) NOT NULL,
    [avatar_location] [varchar](225) NULL,
 CONSTRAINT [PK_USERS] PRIMARY KEY CLUSTERED
(
    [id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]</pre>
