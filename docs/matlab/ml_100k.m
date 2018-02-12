
function ml_100k()
    % plot figures for experiments on ml-100 data collection
    
    function  ml_100k_UserKNN(num, name)
        %% plots for userknn on ml-100k
        h = figure('Name', name);
        N  = num(1:7, 1);          % # of recommendation list
        
        P1 = num(1:7, 2);          % precision
        R1 = num(1:7, 3);          %  recall
        
        P2 = num(8:14, 2);          % precision
        R2 = num(8:14, 3);          %  recall
        
        P3 = num(15:21, 2);          % precision
        R3 = num(15:21, 3);          %  recall
        
        P4 = num(22:28, 2);          % precision
        R4 = num(22:28, 3);          %  recall
        
        P5 = num(29:35, 2);          % precision
        R5 = num(29:35, 3);          %  recall
        
        P6 = num(36:42, 2);          % precision
        R6 = num(36:42, 3);          %  recall
        
        plot(P1, R1,  'bd-',  ...
                P2, R2, 'go-', ...
                P3, R3, 'rx-', ...
                P4, R4, 'c+-', ...
                P5, R5, 'ms-', ...
                P6, R6, 'yv-', ...
                'LineWidth', 2,  'markersize', 8);
            
        x = P5 + 0.01;
        y = R5 + 0.01;
        for i=1:length(x)
           text(x(i), y(i), num2str(N(i)), 'FontSize', 16)
        end
        
        title('UserKNN on ml-100k');
        xlabel('Precision@N');
        ylabel('Recall@N');
        legend('K=5', 'K=10',  'K=20',  'K=40',  'K=80',  'K=160');
        grid on;
        %set(h,'ytick',y_axis_set);  % 设置刻度线
        %set(h,'yticklabel',y_axis_set); %设置显示哪些刻度值
        set(gca,'FontSize',16);  % 设置坐标轴数字，图例，标题等字体大小
        %set(get(gca,'XLabel'),'FontSize',18);  % 设置x轴标签字体大小
        %set(get(gca,'YLabel'), 'FontSize',18);  % 设置y轴标签字体大小
        
        %print(h, strcat('img\', name, '_UserKNN'), '-depsc');
        print(h, strcat('img\', name, '_UserKNN'), '-dpdf');
    end

    function  ml_100k_ItemKNN(num, name)
            %% plots for itemKNN on ml-100k
            h = figure('Name', name);
            N  = num(1:7, 1);          % # of recommendation list

            P1 = num(1:7, 2);          % precision
            R1 = num(1:7, 3);          %  recall

            P2 = num(8:14, 2);          % precision
            R2 = num(8:14, 3);          %  recall

            P3 = num(15:21, 2);          % precision
            R3 = num(15:21, 3);          %  recall

            P4 = num(22:28, 2);          % precision
            R4 = num(22:28, 3);          %  recall

            P5 = num(29:35, 2);          % precision
            R5 = num(29:35, 3);          %  recall

            P6 = num(36:42, 2);          % precision
            R6 = num(36:42, 3);          %  recall

            plot(P1, R1,  'bd-',  ...
                    P2, R2, 'go-', ...
                    P3, R3, 'rx-', ...
                    P4, R4, 'c+-', ...
                    P5, R5, 'ms-', ...
                    P6, R6, 'yv-', ...
                    'LineWidth', 2,  'markersize', 8);
            
            x = P3 + 0.01;
            y = R3 + 0.01;
            for i=1:length(x)
                text(x(i), y(i), num2str(N(i)), 'FontSize', 16)
            end

            title('ItemKNN on ml-100k');
            xlabel('Precision@N');
            ylabel('Recall@N');
            legend('K=5', 'K=10',  'K=20',  'K=40',  'K=80',  'K=160');
            grid on;
            %set(h,'ytick',y_axis_set);  % 设置刻度线
            %set(h,'yticklabel',y_axis_set); %设置显示哪些刻度值
            set(gca,'FontSize',16);  % 设置坐标轴数字，图例，标题等字体大小
            %set(get(gca,'XLabel'),'FontSize',18);  % 设置x轴标签字体大小
            %set(get(gca,'YLabel'), 'FontSize',18);  % 设置y轴标签字体大小
            
            %print(h, strcat('img\', name, '_ItemKNN'), '-depsc');
            print(h, strcat('img\', name, '_ItemKNN'), '-dpdf');
    end

    function  ml_100k_MF(num, name)
        %% plots for MF on ml-100k
        h = figure('Name', name);
        N  = num(1:7, 1);          % # of recommendation list
        
        P1 = num(1:7, 2);          % precision
        R1 = num(1:7, 3);          %  recall
        
        P2 = num(8:14, 2);          % precision
        R2 = num(8:14, 3);          %  recall
        
        P3 = num(15:21, 2);          % precision
        R3 = num(15:21, 3);          %  recall
        
        P4 = num(22:28, 2);          % precision
        R4 = num(22:28, 3);          %  recall
        
        P5 = num(29:35, 2);          % precision
        R5 = num(29:35, 3);          %  recall
        
        P6 = num(36:42, 2);          % precision
        R6 = num(36:42, 3);          %  recall
        
        P7 = num(43:49, 2);          % precision
        R7 = num(43:49, 3);          %  recall
        
        P8 = num(50:56, 2);          % precision
        R8 = num(50:56, 3);          %  recall
        
        P9 = num(57:63, 2);          % precision
        R9 = num(57:63, 3);          %  recall
        
        P10 = num(64:70, 2);          % precision
        R10 = num(64:70, 3);          %  recall
        
        plot(P1, R1,  'bd-',  ...
                P2, R2, 'go-', ...
                P3, R3, 'rx-', ...
                P4, R4, 'c+-', ...
                P5, R5, 'ms-', ...  
                P6, R6, 'yv-', ...
                P7, R7, 'k^-', ...
                P8, R8, 'm<--', ...  
                P9, R9, 'y>--', ...
                P10, R10, 'kh--', ... 
                'LineWidth', 2,  'markersize', 8);
            
        x = P5 + 0.01;
        y = R5 + 0.01;
        for i=1:length(x)
           text(x(i), y(i), num2str(N(i)), 'FontSize', 16)
        end
        
        title('MF on ml-100k');
        xlabel('Precision@N');
        ylabel('Recall@N');
        legend('ratio=1', 'ratio=2',  'ratio=3',  'ratio=4',  'ratio=5', ...
                     'ratio=6', 'ratio=7',  'ratio=8',  'ratio=9',  'ratio=10');
        grid on;
        %set(h,'ytick',y_axis_set);         % 设置刻度线
        %set(h,'yticklabel',y_axis_set);        %设置显示哪些刻度值
        set(gca,'FontSize',16);  % 设置坐标轴数字，图例，标题等字体大小
        %set(get(gca,'XLabel'),'FontSize',18);      % 设置x轴标签字体大小
        %set(get(gca,'YLabel'), 'FontSize',18);      % 设置y轴标签字体大小
        
        %print(h, strcat('img\', name, '_UserKNN'), '-depsc');
        print(h, strcat('img\', name, '_MF'), '-dpdf');
    end
    
    function  ml_100k_MF_f(num, name)
        %% plots for MF on ml-100k
        h = figure('Name', name);
        N  = num(1:7, 1);          % # of recommendation list
        
        P1 = num(1:7, 2);          % precision
        R1 = num(1:7, 3);          %  recall
        
        P2 = num(8:14, 2);          % precision
        R2 = num(8:14, 3);          %  recall
        
        P3 = num(15:21, 2);          % precision
        R3 = num(15:21, 3);          %  recall
        
        P4 = num(22:28, 2);          % precision
        R4 = num(22:28, 3);          %  recall
        
        P5 = num(29:35, 2);          % precision
        R5 = num(29:35, 3);          %  recall
        
        P6 = num(36:42, 2);          % precision
        R6 = num(36:42, 3);          %  recall

        
        plot(P1, R1,  'bd-',  ...
                P2, R2, 'go-', ...
                P3, R3, 'rx-', ...
                P4, R4, 'c+-', ...
                P5, R5, 'ms-', ...  
                P6, R6, 'yv-', ...
                'LineWidth', 2,  'markersize', 8);
            
        x = P5 + 0.01;
        y = R5 + 0.01;
        for i=1:length(x)
           text(x(i), y(i), num2str(N(i)), 'FontSize', 16)
        end
        
        title('MF on ml-100k(ratio = 4)');
        xlabel('Precision@N');
        ylabel('Recall@N');
        legend('f=10', 'f=20',  'f=50',  'f=100',  'f=200',  'f=600');
        grid on;
        %set(h,'ytick',y_axis_set);         % 设置刻度线
        %set(h,'yticklabel',y_axis_set);        %设置显示哪些刻度值
        set(gca,'FontSize',16);  % 设置坐标轴数字，图例，标题等字体大小
        %set(get(gca,'XLabel'),'FontSize',18);      % 设置x轴标签字体大小
        %set(get(gca,'YLabel'), 'FontSize',18);      % 设置y轴标签字体大小
        
        %print(h, strcat('img\', name, '_UserKNN'), '-depsc');
        print(h, strcat('img\', name, '_MF_f'), '-dpdf');
    end

    function  ml_100k_SLIM(num, name)
        %% plots for MF on ml-100k
        h = figure('Name', name);
        N  = num(1:7, 1);          % # of recommendation list
        
        P1 = num(1:7, 2);          % precision
        R1 = num(1:7, 3);          %  recall
        
        plot(P1, R1,  'bd-',  'LineWidth', 2,  'markersize', 8);
%                 P2, R2, 'go-', ...
%                 P3, R3, 'rx-', ...
%                 P4, R4, 'c+-', ...
%                 P5, R5, 'ms-', ...  
%                 P6, R6, 'yv-', ...
%                 P7, R7, 'k^-', ...
%                 P8, R8, 'm<--', ...  
%                 P9, R9, 'y>--', ...
%                 P10, R10, 'kh--', ... 
                
            
        x = P1 + 0.01;
        y = R1 + 0.01;
        for i=1:length(x)
           text(x(i), y(i), num2str(N(i)), 'FontSize', 16)
        end
        
        title('SLIM on ml-100k');
        xlabel('Precision@N');
        ylabel('Recall@N');
%         legend('ratio=1', 'ratio=2',  'ratio=3',  'ratio=4',  'ratio=5', ...
%                      'ratio=6', 'ratio=7',  'ratio=8',  'ratio=9',  'ratio=10');
        grid on;
        %set(h,'ytick',y_axis_set);         % 设置刻度线
        %set(h,'yticklabel',y_axis_set);        %设置显示哪些刻度值
        set(gca,'FontSize',16);  % 设置坐标轴数字，图例，标题等字体大小
        %set(get(gca,'XLabel'),'FontSize',18);      % 设置x轴标签字体大小
        %set(get(gca,'YLabel'), 'FontSize',18);      % 设置y轴标签字体大小
        
        %print(h, strcat('img\', name, '_UserKNN'), '-depsc');
        print(h, strcat('img\', name, '_SLIM'), '-dpdf');
    end

    function  ml_100k_BPRMF(num, name)
        %% plots for MF on ml-100k
        h = figure('Name', name);
        N  = num(1:7, 1);          % # of recommendation list
        
        P1 = num(1:7, 2);          % precision
        R1 = num(1:7, 3);          %  recall
        
        plot(P1, R1,  'bd-',  'LineWidth', 2,  'markersize', 8);
%                 P2, R2, 'go-', ...
%                 P3, R3, 'rx-', ...
%                 P4, R4, 'c+-', ...
%                 P5, R5, 'ms-', ...  
%                 P6, R6, 'yv-', ...
%                 P7, R7, 'k^-', ...
%                 P8, R8, 'm<--', ...  
%                 P9, R9, 'y>--', ...
%                 P10, R10, 'kh--', ... 
                
            
        x = P1 + 0.01;
        y = R1 + 0.01;
        for i=1:length(x)
           text(x(i), y(i), num2str(N(i)), 'FontSize', 16)
        end
        
        title('BPRMF on ml-100k');
        xlabel('Precision@N');
        ylabel('Recall@N');
%         legend('ratio=1', 'ratio=2',  'ratio=3',  'ratio=4',  'ratio=5', ...
%                      'ratio=6', 'ratio=7',  'ratio=8',  'ratio=9',  'ratio=10');
        grid on;
        %set(h,'ytick',y_axis_set);         % 设置刻度线
        %set(h,'yticklabel',y_axis_set);        %设置显示哪些刻度值
        set(gca,'FontSize',16);  % 设置坐标轴数字，图例，标题等字体大小
        %set(get(gca,'XLabel'),'FontSize',18);      % 设置x轴标签字体大小
        %set(get(gca,'YLabel'), 'FontSize',18);      % 设置y轴标签字体大小
        
        %print(h, strcat('img\', name, '_UserKNN'), '-depsc');
        print(h, strcat('img\', name, '_BPRMF'), '-dpdf');
    end

    clc;
    clear;
    close all;
 
    %% UserKNN
%     num = xlsread('ml-100k',  1, 'B3:F44'); % u1
%     ml_100k_UserKNN(num, 'ml_100_u1');
%     
%     num = xlsread('ml-100k',  1, 'I3:M44'); % u2
%     ml_100k_UserKNN(num, 'ml_100_u2');
%     
%     num = xlsread('ml-100k',  1, 'P3:T44'); % u3
%     ml_100k_UserKNN(num, 'ml_100_u3');
%     
%     num = xlsread('ml-100k',  1, 'W3:AA44'); % u4
%     ml_100k_UserKNN(num, 'ml_100_u4');
%     
%     num = xlsread('ml-100k',  1, 'AD3:AH44'); % u5
%     ml_100k_UserKNN(num, 'ml_100_u5');
    
    %% ItemKNN
%     num = xlsread('ml-100k',  2, 'B3:F44'); % u1
%     ml_100k_ItemKNN(num, 'ml_100_u1');
%     
%     num = xlsread('ml-100k',  2, 'I3:M44'); % u2
%     ml_100k_ItemKNN(num, 'ml_100_u2');
%     
%     num = xlsread('ml-100k',  2, 'P3:T44'); % u3
%     ml_100k_ItemKNN(num, 'ml_100_u3');
%     
%     num = xlsread('ml-100k',  2, 'W3:AA44'); % u4
%     ml_100k_ItemKNN(num, 'ml_100_u4');
%     
%     num = xlsread('ml-100k',  2, 'AD3:AH44'); % u5
%     ml_100k_ItemKNN(num, 'ml_100_u5');

    %% Matrix factorization
%     num = xlsread('ml-100k',  3, 'B3:G72'); % u1
%     ml_100k_MF(num, 'ml_100_u1');
%     
%     num = xlsread('ml-100k',  3, 'J3:O72'); % u1
%     ml_100k_MF(num, 'ml_100_u2');
%     
%     num = xlsread('ml-100k',  3, 'P3:W72'); % u1
%     ml_100k_MF(num, 'ml_100_u3');
%     
%     num = xlsread('ml-100k',  3, 'Z3:AE72'); % u1
%     ml_100k_MF(num, 'ml_100_u4');
%    
%     num = xlsread('ml-100k',  3, 'AH3:AM72'); % u1
%     ml_100k_MF(num, 'ml_100_u5');
%     

%     num = xlsread('ml-100k',  3, 'B77:G118'); % u1
%     ml_100k_MF_f(num, 'ml_100_u1');
%     
%     num = xlsread('ml-100k',  3, 'J77:O118'); % u1
%     ml_100k_MF_f(num, 'ml_100_u2');
%     
%     num = xlsread('ml-100k',  3, 'P77:W118'); % u1
%     ml_100k_MF_f(num, 'ml_100_u3');
%     
%     num = xlsread('ml-100k',  3, 'Z77:AE118'); % u1
%     ml_100k_MF_f(num, 'ml_100_u4');
%     
%     num = xlsread('ml-100k',  3, 'AH77:AM118'); % u1
%     ml_100k_MF_f(num, 'ml_100_u5');

    %% SLIM
    num = xlsread('ml-100k',  4, 'A3:F9'); % u1
    ml_100k_SLIM(num, 'ml_100_u1');
    
%     num = xlsread('ml-100k',  4, 'H3:M9'); % u1
%     ml_100k_SLIM(num, 'ml_100_u2');
%     
%     num = xlsread('ml-100k',  4, 'O3:T9'); % u1
%     ml_100k_SLIM(num, 'ml_100_u3');

    %% BPRMF
    num = xlsread('ml-100k',  5, 'A3:F9'); % u1
    ml_100k_BPRMF(num, 'ml_100_u1');
    
%     num = xlsread('ml-100k',  5, 'H3:M9'); % u1
%     ml_100k_BPRMF(num, 'ml_100_u1');

end



