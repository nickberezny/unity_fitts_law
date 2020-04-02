clc
clear

data = importdata('data.txt')
target = importdata('target.txt')

size = length(data(:,1));
ae = zeros(size-1,2);

%A = pi/4.*target(:,1);

for i = 1:size-1
   
    num = data(i+1,1);
    D(i) = sqrt((data(i,3)-target(num,2))^2 + (data(i,4)-target(num,3))^2);
    MT(i) = data(i+1,2) - data(i,2);
    
    A = pi/4*target(num,1);
    ID(i) = log2(D(i)/A + 1);
    
    xo = [data(i,3),data(i,4)];
    xf = [data(i+1,3),data(i+1,4)];
    xd = [target(num,2),target(num,3)];
    
    ae(i,:) = dot(xf-xo,xd-xo)/dot(xd-xo,xd-xo)*(xd-xo)'; %effective amplitude
    dx(i) = norm(ae(i,:)) - norm(xd-xo);
    
    xt(i) = norm(xd-xo);
    
end


TP = log2(mean(vecnorm(ae'))/(4.1333*std(dx)) + 1)./mean(MT);
%ID = log2(D./A' + 1);

X = [ID',ones(size-1,1)];
Y = MT';
b = X\Y;

figure
plot(ID,MT,'*')
hold on
plot([0 5],b'*[0 5; 1 1])
ylabel('MT (s)')
xlabel('ID (bits)')



